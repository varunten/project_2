# IPMS — How the project works

This document explains the five projects in the solution, what each one is
responsible for, and how data flows through them for every part of the system.

---

## 1. The five projects at a glance

| Project | Type | Responsibility | References |
| - | - | - | - |
| **ipms** | ASP.NET Core Web API | The REST API. Receives HTTP requests, checks authentication/roles, returns JSON. **No business rules.** | ipms.BAL, ipms.DAL |
| **ipms.BAL** | Class library | Business Logic Layer. All insurance rules: premium calculation, quote workflow, claim settlement, renewal. | ipms.DTO, ipms.DAL |
| **ipms.DAL** | Class library | Data Access Layer. The EF Core `DbContext` and repositories. Only this project talks to SQL Server. | ipms.DTO |
| **ipms.DTO** | Class library | Shared "shapes": entities (database tables), DTOs (API request/response), enums, exceptions. **No dependencies at all.** | — |
| **ipms.MVC** | ASP.NET Core MVC | The website the user clicks. Talks to the API over HTTP. Knows nothing about the database. | ipms.DTO only |

### How they depend on each other

```
ipms.MVC ──HTTP──►  ipms (API)
   │                   │
   │                   ▼
   │                ipms.BAL
   │                   │
   │                   ▼
   │                ipms.DAL ──► SQL Server (InsuranceAPIDB)
   │                   │
   └───────────────────┴──► ipms.DTO   (everyone shares the shapes)
```

Two rules keep this clean:

1. **Each layer only talks to the one below it.** A controller never touches the
   database; a repository never knows about HTTP.
2. **ipms.MVC does NOT reference ipms.BAL or ipms.DAL.** It only references
   ipms.DTO (to reuse the request/response classes) and calls the API over HTTP.
   This matters because all the role rules (`[Authorize(Roles = ...)]`) live on
   the API controllers — if the MVC called the BAL directly it would bypass
   every one of them.

---

## 2. ipms.DTO — the shared shapes

The foundation. It references nothing, so every other project can use it.

| Folder | What's inside | Example |
| - | - | - |
| `Entities/` | One class per database table | `Policy`, `Claim`, `AuditLog` |
| `Dtos/` | What the API accepts and returns | `CreateQuoteDto`, `PolicyDto`, `ApiResponse<T>` |
| `Enum/` | Fixed value lists | `PolicyStatus`, `ClaimStatus` |
| `Exceptions/` | Typed errors carrying an HTTP status | `NotFoundException` (404) |
| `Roles.cs` | Role name constants | `Roles.Underwriter` |

**Entities vs DTOs** — an entity is the database row (`Policy` has `DeletedAt`,
`SSNHash`, etc.). A DTO is what goes over the wire (`PolicyDto` has
`ProductName`, and never exposes secrets). The BAL converts between them.

**Validation lives here.** DTOs carry DataAnnotations:

```csharp
[Required(AllowEmptyStrings = false)]
[EmailAddress]
public required string Email { get; set; }
```

---

## 3. ipms.DAL — the Data Access Layer

Only this project knows SQL Server exists.

**`AppDbContext`** is the EF Core context: one `DbSet<>` per table, plus three
things it does automatically on every save:

1. **Timestamps** — sets `CreatedAt`/`UpdatedAt` so no service has to remember.
2. **Audit logging** — writes one `AuditLog` row per insert/update/delete
   (what changed, who changed it, when), in the same transaction.
3. **Column rules** — `decimal(18,2)` for money, `nvarchar(256)` for strings.

**Repositories** (one per area: `PolicyRepository`, `ClaimRepository`, …) hold
the queries. They return **entities**, never DTOs — mapping is the BAL's job.

```csharp
public async Task<Policy?> GetByIdForCustomerAsync(Guid policyId, Guid customerId)
{
    return await _context.Policies
        .FirstOrDefaultAsync(p =>
            p.Id == policyId &&
            p.CustomerId == customerId &&      // ownership check
            p.DeletedAt == null);
}
```

`ICurrentUserProvider` is an interface defined here and implemented in the API —
that's how the DbContext learns *who* is making a change for the audit log,
without the data layer having to know about HTTP.

---

## 4. ipms.BAL — the Business Logic Layer

Where the insurance rules live. Each service takes repositories in its
constructor, applies rules, maps entities to DTOs, and throws typed exceptions
when a rule is broken.

```csharp
if (quote.Status != QuoteStatus.AcceptedByCustomer)
    throw new ConflictException("Only quotes accepted by the customer can be approved.");
```

Services never return HTTP status codes — they throw, and the API's middleware
turns the exception into the right status. Examples of rules held here:

- Premium = `BasePremium × (requested coverage ÷ product coverage)`
- A quote must be accepted by the customer before an underwriter may approve it
- Approving a quote issues a policy **and** starts its premium schedule
- Paying an installment queues the next one until the policy term is covered
- A policy past its end date closes itself (`Expired`)
- A cancelled policy cannot be renewed; a policy can only be renewed once

---

## 5. ipms — the Web API

Thin controllers. A typical action is three lines: get the user id, call the
service, wrap the result.

```csharp
[HttpPatch("{quote_id}/approve")]
[Authorize(Roles = Roles.Underwriter)]
public async Task<ActionResult<ApiResponse<PolicyDto>>> ApproveQuote(Guid quote_id)
{
    PolicyDto result = await _service.ApproveQuoteAsync(GetUserId(), quote_id);
    return Ok(ApiResponse.Ok(result, "Quote approved and policy issued."));
}
```

**The request pipeline** (order matters — set in `Program.cs`):

```
Request
  ├─ ExceptionHandlingMiddleware   catches everything, writes ErrorLogs
  ├─ Authentication                validates the JWT
  ├─ SessionValidationMiddleware   rejects revoked/logged-out sessions
  ├─ Authorization                 enforces [Authorize(Roles = ...)]
  └─ Controller ─► Service ─► Repository ─► SQL Server
```

**Every response has the same shape:**

```jsonc
// success
{ "success": true, "message": "Policy renewed.", "data": { ... } }

// error
{ "success": false, "message": "Policy not found.", "errors": null }

// validation error (422) — errors is filled per field
{ "success": false, "message": "Validation failed...",
  "errors": { "Email": ["Email is not a valid email address."] } }
```

On startup it also runs migrations and seeds the four roles plus a bootstrap
admin (`admin@ipms.local` / `Admin@123`).

---

## 6. ipms.MVC — the website

Server-rendered Razor pages using Bootstrap. It is just another **client** of
the API — the same way a mobile app would be.

| Piece | Job |
| - | - |
| `IpmsApiClient` | The only class that calls the API. Unwraps `ApiResponse<T>`, throws `ApiException` on failure. |
| `AuthTokenHandler` | Attaches `Authorization: Bearer <token>` to every outgoing call automatically. |
| `JwtHelper` | Reads roles out of the token so the menu shows the right links. |
| `BaseController` | Shared helpers, incl. turning API 422 errors into form errors. |

**Login flow:** the user posts the form → MVC calls `POST /api/auth/login` →
the JWT is stored in **Session** → every later call picks it up via the handler.

---

## 7. Data flow — walking through the system

### 7.1 The shape of every request

```
Browser ──► MVC Controller ──► IpmsApiClient ──HTTP+JWT──► API Controller
                                                              │
                                                              ▼
                                                        BAL Service   (rules)
                                                              │
                                                              ▼
                                                        Repository    (query)
                                                              │
                                                              ▼
                                                         SQL Server
```
…and the response travels back the same way: entity → DTO (in the BAL) →
`ApiResponse<T>` JSON (in the API) → Razor view (in the MVC).

### 7.2 Sign-up and login

```
POST /api/auth/signup   AuthService: check duplicates, hash password,
                        create User + assign "Customer" role
POST /api/auth/login    verify password → create TokenFamily + RefreshToken
                        → build JWT containing userId, email, sessionId, roles
```
The JWT's **roles** are what make `[Authorize(Roles = ...)]` work. A user who is
promoted must log in again for the new role to appear in their token.

### 7.3 The core insurance workflow

```
1. CUSTOMER  POST /api/customer            create profile (required before quoting)
2. CUSTOMER  GET  /api/product             browse the catalogue
3. CUSTOMER  POST /api/quote               request a quote
                                           → premium calculated, status = Requested
4. CUSTOMER  PATCH /api/quote/{id}/accept  → status = AcceptedByCustomer
5. UNDERWRITER GET /api/quote/pending      the review queue
6. UNDERWRITER PATCH /api/quote/{id}/approve
                                           → quote = Approved
                                           → POLICY created (status Active)
                                           → first premium installment created
             (or /reject → status = Rejected)
```

Everything in step 6 happens in **one transaction** — the quote update, the new
policy and the first installment share a single `SaveChanges`, so either all of
it happens or none of it does.

### 7.4 Premium payments

```
GET  /api/premiumpayment/policy/{id}   the schedule (own policies only)
POST /api/premiumpayment/{id}/pay      customer pays
        │
        ├─ must pay at least premium + penalty
        ├─ status = Success, or Late if paid after the due date
        └─ the NEXT installment is created automatically
           (spaced by frequency, stops at the policy end date)
```

### 7.5 Claims

```
CUSTOMER    POST  /api/claim            file a claim
                                        (must own the policy, policy must be Active)
CUSTOMER    GET   /api/claim/my         their own claims
UNDERWRITER GET   /api/claim            the review queue
UNDERWRITER PATCH /api/claim/{id}       approve / reject
                                        → Approved sets ApprovedAmount + SettledDate
```

### 7.6 Policy closure and renewal

```
CLOSURE   any Active policy whose EndDate has passed becomes Expired
          (checked whenever policies are read - no background job needed)

CANCEL    PATCH /api/policy/{id}/cancel  customer cancels an Active policy

RENEWAL   POST  /api/policy/{id}/renew
             ├─ new policy linked by PreviousPolicyId
             ├─ starts where the old one ends (no gap in cover)
             ├─ gets its own first installment
             └─ blocked if cancelled, or already renewed
```

### 7.7 Logging — the two audit tables

Both are written automatically; no service calls them.

```
AuditLogs   written by AppDbContext.SaveChanges
            one row per insert/update/delete
            → who, which table, which row, which columns changed

ErrorLogs   written by ExceptionHandlingMiddleware
            one row per exception (business 4xx and unexpected 500)
            → message (incl. the inner SQL error), type, stack trace,
              path, method, status code, user
```

The error logger deliberately uses a **fresh DbContext**: if the exception came
out of a failed `SaveChanges`, the request's own context still holds the broken
changes, so reusing it would fail again.

---

## 8. Roles — who can do what

| Role | Can do |
| - | - |
| **Customer** | Create profile, browse products, request/accept quotes, view own policies, pay premiums, file claims, renew |
| **Underwriter** | See the pending-quote queue, approve/reject quotes, review and settle claims |
| **InsuranceAgent** | Create/update premium installments |
| **Admin** | Manage products, list all users, create staff accounts, promote users |

Enforced in **two** places: the API (`[Authorize(Roles = ...)]` — the real
enforcement) and the MVC (hiding links/buttons — convenience only). The UI check
is cosmetic; the API is what actually protects the data.

---

## 9. Running it

Two terminals — the API must be up first, because the website calls it.

```bash
# Terminal 1 - API
dotnet run --project ipms --launch-profile http        # http://localhost:5128

# Terminal 2 - website
dotnet run --project ipms.MVC --launch-profile http    # http://localhost:5146
```

Database (SQL Server Express, `.\SQLEXPRESS`, database `InsuranceAPIDB`):

```bash
dotnet ef database update --project ipms.DAL --startup-project ipms
```

See `info.txt` for the full command reference.
