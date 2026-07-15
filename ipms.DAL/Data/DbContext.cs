using IPMS.DTO.Entities;
using Microsoft.EntityFrameworkCore;

namespace IPMS.DAL.Data;


public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {
        
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
    }


    // Automatically stamp CreatedAt / UpdatedAt on every save so no service
    // has to remember to set them.
    public override int SaveChanges()
    {
        ApplyTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
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
}