using Ecommerce.Promotions.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Promotions.Infrastructure;

public class PromotionsDbContext : DbContext
{
    public PromotionsDbContext(DbContextOptions<PromotionsDbContext> options)
        : base(options) { }

    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<PromotionUse> PromotionUses => Set<PromotionUse>();
    public DbSet<FlashSale> FlashSales => Set<FlashSale>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("promotions");

        modelBuilder.Entity<Campaign>(e =>
        {
            e.ToTable("campaigns");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.Name).HasColumnName("name").IsRequired();
            e.Property(c => c.Type).HasColumnName("type").HasDefaultValue("seasonal").IsRequired();
            e.Property(c => c.StartsAt).HasColumnName("starts_at").IsRequired();
            e.Property(c => c.EndsAt).HasColumnName("ends_at").IsRequired();
            e.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(c => c.Priority).HasColumnName("priority").HasDefaultValue(0);
        });

        modelBuilder.Entity<Promotion>(e =>
        {
            e.ToTable("promotions");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.CampaignId).HasColumnName("campaign_id");
            e.Property(p => p.Code).HasColumnName("code");
            e.Property(p => p.DiscountType).HasColumnName("discount_type").IsRequired();
            e.Property(p => p.DiscountValue).HasColumnName("discount_value")
                .HasColumnType("numeric(10,4)").HasDefaultValue(0m);
            e.Property(p => p.MinOrderAmount).HasColumnName("min_order_amount")
                .HasColumnType("numeric(12,2)").HasDefaultValue(0m);
            e.Property(p => p.MaxUses).HasColumnName("max_uses");
            e.Property(p => p.UsesCount).HasColumnName("uses_count").HasDefaultValue(0);
            e.Property(p => p.MaxUsesPerUser).HasColumnName("max_uses_per_user").HasDefaultValue(1);
            e.Property(p => p.Stackable).HasColumnName("stackable").HasDefaultValue(false);
            e.Property(p => p.ExpiresAt).HasColumnName("expires_at");
            e.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            e.HasIndex(p => p.Code).IsUnique().HasFilter("code IS NOT NULL");

            e.HasMany(p => p.Uses)
                .WithOne()
                .HasForeignKey(u => u.PromotionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PromotionUse>(e =>
        {
            e.ToTable("promotion_uses");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.PromotionId).HasColumnName("promotion_id").IsRequired();
            e.Property(u => u.UserId).HasColumnName("user_id").IsRequired();
            e.Property(u => u.OrderId).HasColumnName("order_id").IsRequired();
            e.Property(u => u.DiscountApplied).HasColumnName("discount_applied")
                .HasColumnType("numeric(12,2)").IsRequired();
            e.Property(u => u.UsedAt).HasColumnName("used_at").HasDefaultValueSql("NOW()");
            e.HasIndex(u => new { u.PromotionId, u.OrderId }).IsUnique();
        });

        modelBuilder.Entity<FlashSale>(e =>
        {
            e.ToTable("flash_sales");
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).HasColumnName("id");
            e.Property(f => f.CampaignId).HasColumnName("campaign_id");
            e.Property(f => f.VariantId).HasColumnName("variant_id").IsRequired();
            e.Property(f => f.SalePrice).HasColumnName("sale_price")
                .HasColumnType("numeric(12,2)").IsRequired();
            e.Property(f => f.StockLimit).HasColumnName("stock_limit");
            e.Property(f => f.SoldCount).HasColumnName("sold_count").HasDefaultValue(0);
            e.Property(f => f.StartsAt).HasColumnName("starts_at").IsRequired();
            e.Property(f => f.EndsAt).HasColumnName("ends_at").IsRequired();
            e.Ignore(f => f.IsActive);
        });
    }
}
