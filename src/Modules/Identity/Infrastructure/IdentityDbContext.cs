using Ecommerce.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Identity.Infrastructure;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.Email).HasColumnName("email").IsRequired().HasMaxLength(320);
            e.Property(u => u.FullName).HasColumnName("full_name").IsRequired();
            e.Property(u => u.Phone).HasColumnName("phone");
            e.Property(u => u.Role).HasColumnName("role").HasDefaultValue("buyer").IsRequired();
            e.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
            e.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            e.Property(u => u.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            e.HasIndex(u => u.Email).IsUnique();
        });
    }
}
