using Ecommerce.Orders.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Orders.Infrastructure;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("orders");

        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.HasKey(o => o.Id);
            e.Property(o => o.Id).HasColumnName("id");
            e.Property(o => o.UserId).HasColumnName("user_id").IsRequired();
            e.Property(o => o.AddressId).HasColumnName("address_id").IsRequired();
            e.Property(o => o.Status).HasColumnName("status").HasDefaultValue("pending").IsRequired();
            e.Property(o => o.TotalAmount).HasColumnName("total_amount")
                .HasColumnType("numeric(12,2)").IsRequired();
            e.Property(o => o.CurrencyCode).HasColumnName("currency_code")
                .HasDefaultValue("EUR").IsRequired();
            e.Property(o => o.TotalInBase).HasColumnName("total_in_base")
                .HasColumnType("numeric(12,2)");
            e.Property(o => o.BaseCurrency).HasColumnName("base_currency")
                .HasDefaultValue("EUR").IsRequired();
            e.Property(o => o.FxRateSnapshot).HasColumnName("fx_rate_snapshot")
                .HasColumnType("numeric(18,8)");
            e.Property(o => o.OrderedAt).HasColumnName("ordered_at")
                .HasDefaultValueSql("NOW()");
            e.Property(o => o.UpdatedAt).HasColumnName("updated_at")
                .HasDefaultValueSql("NOW()");

            e.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("order_items");
            e.HasKey(i => i.Id);
            e.Property(i => i.Id).HasColumnName("id");
            e.Property(i => i.OrderId).HasColumnName("order_id").IsRequired();
            e.Property(i => i.VariantId).HasColumnName("variant_id").IsRequired();
            e.Property(i => i.ProductId).HasColumnName("product_id");
            e.Property(i => i.SellerId).HasColumnName("seller_id");
            e.Property(i => i.ProductName).HasColumnName("product_name").IsRequired();
            e.Property(i => i.ProductSku).HasColumnName("product_sku").IsRequired();
            e.Property(i => i.Quantity).HasColumnName("quantity").IsRequired();
            e.Property(i => i.UnitPrice).HasColumnName("unit_price")
                .HasColumnType("numeric(12,2)").IsRequired();
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.ToTable("payments");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.OrderId).HasColumnName("order_id").IsRequired();
            e.Property(p => p.Method).HasColumnName("method").IsRequired();
            e.Property(p => p.Status).HasColumnName("status").HasDefaultValue("pending").IsRequired();
            e.Property(p => p.Amount).HasColumnName("amount")
                .HasColumnType("numeric(12,2)").IsRequired();
            e.Property(p => p.TransactionRef).HasColumnName("transaction_ref");
            e.Property(p => p.PaidAt).HasColumnName("paid_at");
            e.Property(p => p.CurrencyCode).HasColumnName("currency_code");
            e.Property(p => p.AmountInBase).HasColumnName("amount_in_base")
                .HasColumnType("numeric(12,2)");
            e.Property(p => p.FxRate).HasColumnName("fx_rate")
                .HasColumnType("numeric(18,8)");
            e.HasIndex(p => p.OrderId).IsUnique();
        });
    }
}
