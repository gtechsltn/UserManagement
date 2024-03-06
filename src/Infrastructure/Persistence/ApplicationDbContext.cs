using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=UserDb;Trusted_Connection=True;MultipleActiveResultSets=true;");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

public class ApplicationDbContext : IdentityDbContext<User>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = "API";
                    entry.Entity.Created = DateTime.Now;
                    entry.Entity.LastModified = DateTime.Now;
                    entry.Entity.LastModifiedBy = "API";
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModified = DateTime.Now;
                    entry.Entity.LastModifiedBy = "API";
                    break;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }

}