using IPMS.DTO.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IPMS.DAL.Data;


public class AppDbContext: DbContext
{
    // Used to record "who" on each audit row. Optional so the context can still
    // be created without it (design-time tooling, tests).
    private readonly ICurrentUserProvider? _currentUser;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserProvider? currentUser = null): base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<User> Users {get; set;}
    public DbSet<Role> Roles {get; set;}
    public DbSet<UserRole> UserRoles {get; set;}
    public DbSet<RefreshToken> RefreshTokens {get; set;}
    public DbSet<TokenFamily> TokenFamilies {get; set;}
    public DbSet<Customer> Customers {get; set;}
    public DbSet<CustomerAddress> CustomerAddresses {get; set;}
    public DbSet<Product> Products {get; set;}
    public DbSet<Quote> Quotes {get; set;}
    public DbSet<Policy> Policies {get; set;}
    public DbSet<PremiumPayment> PremiumPayments {get; set;}
    public DbSet<Claim> Claims {get; set;}
    public DbSet<ClaimDocument> ClaimDocuments {get; set;}
    public DbSet<AuditLog> AuditLogs {get; set;}
    public DbSet<ErrorLog> ErrorLogs {get; set;}


    // Give every decimal a money-friendly precision (18 digits, 2 decimals).
    // Without this, SQL Server defaults would silently truncate amounts.
    // Strings default to nvarchar(256) instead of nvarchar(max), which is
    // smaller and (unlike max) can actually be indexed.
    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        configurationBuilder.Properties<string>().HaveMaxLength(256);
    }


    // The few free-text columns that need more room than the 256 default.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().Property(p => p.Description).HasMaxLength(1000);
        modelBuilder.Entity<Quote>().Property(q => q.Remarks).HasMaxLength(1000);
        modelBuilder.Entity<Policy>().Property(p => p.CancellationReason).HasMaxLength(1000);
        modelBuilder.Entity<Claim>().Property(c => c.Reason).HasMaxLength(1000);
        modelBuilder.Entity<Claim>().Property(c => c.Notes).HasMaxLength(2000);
        modelBuilder.Entity<ClaimDocument>().Property(d => d.FileURL).HasMaxLength(2048);

        // The changed-columns list can be longer than the 256 default.
        modelBuilder.Entity<AuditLog>().Property(a => a.ChangedColumns).HasMaxLength(1000);

        // Error text can be long - a stack trace must never be truncated by the
        // 256 default, otherwise writing the log would itself fail.
        modelBuilder.Entity<ErrorLog>().Property(e => e.Message).HasMaxLength(2000);
        modelBuilder.Entity<ErrorLog>().Property(e => e.StackTrace).HasColumnType("nvarchar(max)");
        modelBuilder.Entity<ErrorLog>().Property(e => e.Path).HasMaxLength(512);
        modelBuilder.Entity<ErrorLog>().Property(e => e.Method).HasMaxLength(16);
    }


    // Automatically stamp CreatedAt / UpdatedAt AND record an audit trail on
    // every save, so no service has to remember to do either.
    public override int SaveChanges()
    {
        ApplyTimestamps();
        CaptureAuditLogs();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        CaptureAuditLogs();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyTimestamps()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }


    // Adds one AuditLog row for every insert / update / delete about to be
    // saved. Runs before base.SaveChanges so it is committed in the same
    // transaction as the change it describes.
    private void CaptureAuditLogs()
    {
        // Snapshot the changes first - we must not audit the log rows we add
        // (that would recurse), and adding to the context would otherwise
        // change this collection while we iterate it.
        List<EntityEntry> entries = ChangeTracker.Entries()
            .Where(e =>
                e.Entity is not AuditLog &&
                e.Entity is not ErrorLog &&
                (e.State == EntityState.Added ||
                 e.State == EntityState.Modified ||
                 e.State == EntityState.Deleted))
            .ToList();

        if (entries.Count == 0)
            return;

        Guid? userId = _currentUser?.GetUserId();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (EntityEntry entry in entries)
        {
            string action = entry.State switch
            {
                EntityState.Added => "Added",
                EntityState.Deleted => "Deleted",
                _ => "Modified"
            };

            string? changedColumns = entry.State == EntityState.Modified
                ? string.Join(", ", entry.Properties
                    .Where(p => p.IsModified)
                    .Select(p => p.Metadata.Name))
                : null;

            AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                TableName = entry.Metadata.GetTableName() ?? entry.Metadata.ClrType.Name,
                RecordId = GetPrimaryKey(entry),
                ChangedColumns = changedColumns,
                Timestamp = now
            });
        }
    }


    private static string GetPrimaryKey(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null)
            return string.Empty;

        IEnumerable<string?> values = key.Properties
            .Select(p => entry.Property(p.Name).CurrentValue?.ToString());

        return string.Join(",", values);
    }
}