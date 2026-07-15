# Setup on a new machine & the ipms.MVC question

## 1) Setting this project up on another machine

The database is **not** part of the repo (no `.db` file, no server backup checked in) — only the *definition* of the database (the EF migrations) is checked in. So setup on a fresh machine is: install SQL Server, point the config at it, run the migration.

### Step 1 — Install SQL Server
Any edition works (Express is fine, that's what's used here). During setup, note down:
- The **instance name** (e.g. `SQLEXPRESS`, or a named instance you chose).
- The **auth mode** — this project uses Windows/Trusted auth, no SQL login needed.

Check what instance name you actually got — it is *not always* the same on every machine:

```powershell
Get-Service | Where-Object { $_.Name -like "*SQL*" }
```

A service named `MSSQL$SQLEXPRESS` means the instance is `.\SQLEXPRESS`. A plain `MSSQLSERVER` service means the instance is just `.` (the default instance, no suffix).

**Important gotcha hit on this machine**: `(localdb)\MSSQLLocalDB` is a *different product* (LocalDB) from SQL Server Express, and isn't installed just because Express is. Don't assume the connection string from one machine works on another — always verify with the command above.

### Step 2 — Update the connection string
In [ipms/appsettings.json](../ipms/appsettings.json), match the instance name from Step 1:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=InsuranceAPIDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

- `Server=.\SQLEXPRESS` → change `SQLEXPRESS` to your instance name, or just `.` for a default instance.
- `Database=InsuranceAPIDB` → the database doesn't need to exist yet; the next step creates it.
- `TrustServerCertificate=True` → needed because the client (Microsoft.Data.SqlClient) defaults to encrypted connections, and a local dev SQL Server has a self-signed certificate.

### Step 3 — Install the EF CLI tool (one-time per machine)
```bash
dotnet tool install --global dotnet-ef --version 10.0.9
```

### Step 4 — Create the database from the migration
The migration files under [ipms.DAL/Migrations](../ipms.DAL/Migrations) already describe every table, column, and foreign key. Nothing needs to be written by hand — just apply them:

```bash
dotnet ef database update --project ipms.DAL --startup-project ipms
```

This connects using the string from Step 2, creates `InsuranceAPIDB` if it doesn't exist, and creates all 13 tables inside it.

### Step 5 — Run the app
```bash
dotnet run --project ipms --launch-profile http
```

On first run, [Program.cs](../ipms/Program.cs) automatically seeds the 4 roles and one bootstrap admin (`admin@ipms.local` / `Admin@123`) — no manual data entry needed.

### Summary — what travels with the repo vs. what doesn't

| Travels with the repo (git) | Machine-specific (never commit) |
| --- | --- |
| Entity classes, migrations (schema definition) | Which SQL Server instance is installed |
| `appsettings.Development.json` structure | The actual connection string values |
| Seed logic (roles, bootstrap admin) | The database's data (rows) |

If you ever need a completely fresh database (e.g. to wipe test data), just `DROP DATABASE InsuranceAPIDB` and re-run Step 4 — the migration recreates it from scratch.


---

## 2) How to build ipms.MVC — call the API, don't rewrite the data layer

Short answer: **call the existing API over HTTP. Don't write new database code, and don't reference `ipms.BAL`/`ipms.DAL` from the MVC project.**

The database being "outside" `ipms` (now on a real SQL Server) doesn't change this answer — it actually reinforces it. A shared SQL Server database can technically be reached from two different apps, but **the authorization rules cannot**:

- Every role/ownership rule (customer-vs-admin-vs-underwriter, "can I approve my own quote", etc.) is written as `[Authorize(Roles = ...)]` attributes **on the API's controllers**.
- None of that enforcement exists inside `ipms.BAL`'s services — the services trust that the caller has already been checked.
- If `ipms.MVC` referenced `ipms.BAL` and called `IQuoteService.ApproveQuoteAsync(...)` directly, it would **bypass every one of those checks silently** — e.g. a customer could approve their own quote, because nothing inside the service itself stops them.

So the architecture is:

```
ipms.MVC  --(HTTP + JWT)-->  ipms (API)  -->  ipms.BAL  -->  ipms.DAL  -->  SQL Server
```

Not:

```
ipms.MVC  -->  ipms.BAL  -->  ipms.DAL  -->  SQL Server      (bypasses all the API's rules)
```

### What ipms.MVC should reference
- **`ipms.DTO` only.** This is the reuse win: the request/response shapes (`ProductDto`, `CreateQuoteDto`, `ApiResponse<T>`, etc.) are already written — the MVC's view models can just be these same classes, no duplicate model classes needed.
- **Not** `ipms.BAL`, **not** `ipms.DAL` — the MVC has no reason to know a database exists at all.

### How the MVC talks to the API
1. A typed `HttpClient` (registered via `IHttpClientFactory`) with base address `http://localhost:5128`.
2. Login page posts to `/api/auth/login`, gets back a JWT (`TokenDto`), and stores it in **Session** (or a cookie) — the MVC becomes just another authenticated client of the API, the same way a mobile app would be.
3. A `DelegatingHandler` reads the token out of session and attaches `Authorization: Bearer <token>` to every outgoing request automatically, so individual controller actions don't have to repeat that.
4. Responses deserialize into `ApiResponse<T>` on success, or `ErrorResponse` on failure — and the 422 validation `errors` dictionary maps directly onto MVC's `ModelState`, so server-side validation messages show up on the form for free.

### Why this doesn't cost much
Running two processes (`ipms` API + `ipms.MVC`) locally is a one-line `dotnet run` in two terminals — no more complex than what's already being done for API testing. In exchange, every authorization rule, every business rule (premium calculation, quote workflow, refresh-token reuse detection, etc.) is written and enforced in exactly one place, and both the API's own Swagger/Scalar clients and the MVC UI get it automatically and consistently.
