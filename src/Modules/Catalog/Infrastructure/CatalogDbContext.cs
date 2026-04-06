using Ecommerce.Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Infrastructure;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Category> Categories => Set<Category>();
<<<<<<< HEAD
<<<<<<< HEAD
    public DbSet<Seller> Sellers => Set<Seller>();
=======
>>>>>>> feature/orders-logic
=======
>>>>>>> feature/notifications-logic

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("products");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.SellerId).HasColumnName("seller_id").IsRequired();
<<<<<<< HEAD
<<<<<<< HEAD
            e.Property(p => p.SellerName).HasColumnName("seller_name").IsRequired().HasMaxLength(500);
            e.Property(p => p.CategoryId).HasColumnName("category_id");
            e.Property(p => p.Name).HasColumnName("name").IsRequired().HasMaxLength(500);
            e.Property(p => p.Slug).HasColumnName("slug").IsRequired().HasMaxLength(500);
            e.Property(p => p.Description).HasColumnName("description");
            e.Property(p => p.BasePrice).HasColumnName("base_price")
                .HasColumnType("numeric(12,2)").IsRequired();
            e.Property(p => p.MainImageUrl).HasColumnName("main_image_url");
            e.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            e.HasIndex(p => p.Slug).IsUnique();
=======
=======
>>>>>>> feature/notifications-logic
            e.Property(p => p.CategoryId).HasColumnName("category_id");
            e.Property(p => p.Name).HasColumnName("name").IsRequired().HasMaxLength(500);
            e.Property(p => p.Description).HasColumnName("description");
            e.Property(p => p.BasePrice).HasColumnName("base_price")
                .HasColumnType("numeric(12,2)").IsRequired();
            e.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(p => p.CreatedAt).HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at")
                .HasDefaultValueSql("NOW()");

<<<<<<< HEAD
>>>>>>> feature/orders-logic
=======
>>>>>>> feature/notifications-logic
            e.Ignore(p => p.Variants);
        });

        modelBuilder.Entity<ProductVariant>(e =>
        {
            e.ToTable("product_variants");
            e.HasKey(v => v.Id);
            e.Property(v => v.Id).HasColumnName("id");
            e.Property(v => v.ProductId).HasColumnName("product_id").IsRequired();
            e.Property(v => v.Sku).HasColumnName("sku").IsRequired();
            e.Property(v => v.Color).HasColumnName("color");
            e.Property(v => v.Size).HasColumnName("size");
            e.Property(v => v.Price).HasColumnName("price").HasColumnType("numeric(12,2)");
            e.Property(v => v.StockQuantity).HasColumnName("stock_quantity").HasDefaultValue(0);
            e.HasIndex(v => v.Sku).IsUnique();
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.ParentId).HasColumnName("parent_id");
            e.Property(c => c.Name).HasColumnName("name").IsRequired();
            e.Property(c => c.Slug).HasColumnName("slug").IsRequired();
            e.HasIndex(c => c.Slug).IsUnique();
<<<<<<< HEAD
<<<<<<< HEAD
            e.HasMany(c => c.Children)
             .WithOne()
             .HasForeignKey(c => c.ParentId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Seller>(e =>
        {
            e.ToTable("sellers");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnName("id");
            e.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
            e.Property(s => s.BusinessName).HasColumnName("business_name").IsRequired().HasMaxLength(500);
            e.Property(s => s.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
            e.Property(s => s.Phone).HasColumnName("phone").HasMaxLength(50);
            e.Property(s => s.Address).HasColumnName("address");
            e.Property(s => s.StripeAccountId).HasColumnName("stripe_account_id").HasMaxLength(255);
            e.Property(s => s.IsVerified).HasColumnName("is_verified").HasDefaultValue(false);
            e.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            e.Property(s => s.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            e.HasIndex(s => s.UserId).IsUnique();
            e.HasIndex(s => s.StripeAccountId).IsUnique().HasFilter("stripe_account_id IS NOT NULL");
=======
>>>>>>> feature/orders-logic
=======
>>>>>>> feature/notifications-logic
        });
    }
}
