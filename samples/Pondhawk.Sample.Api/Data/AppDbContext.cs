using Microsoft.EntityFrameworkCore;
using Pondhawk.Persistence;
using Pondhawk.Sample.Api.Domain;

namespace Pondhawk.Sample.Api.Data;

/// <summary>
/// The application's EF Core context. Implementing <see cref="IDbContext"/> is free — a
/// <see cref="DbContext"/> already provides the matching <c>Set&lt;T&gt;()</c> and
/// <c>SaveChangesAsync</c> members — so slice handlers can depend on the seam instead of this type.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IDbContext
{
    public DbSet<Widget> Widgets => Set<Widget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Widget>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.HasIndex(w => w.Uid).IsUnique();
            entity.Property(w => w.Uid).HasMaxLength(26).IsRequired();
            entity.Property(w => w.Name).HasMaxLength(200).IsRequired();
            entity.Property(w => w.Description).HasMaxLength(2000);
        });
    }
}
