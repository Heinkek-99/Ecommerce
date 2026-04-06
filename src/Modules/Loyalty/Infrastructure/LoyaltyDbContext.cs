using Ecommerce.Loyalty.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Loyalty.Infrastructure;

public class LoyaltyDbContext : DbContext
{
    public LoyaltyDbContext(DbContextOptions<LoyaltyDbContext> options)
        : base(options) { }

    public DbSet<LoyaltyProgram> LoyaltyPrograms => Set<LoyaltyProgram>();
    public DbSet<LoyaltyTier> LoyaltyTiers => Set<LoyaltyTier>();
    public DbSet<LoyaltyAccount> LoyaltyAccounts => Set<LoyaltyAccount>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
    public DbSet<LoyaltyReward> LoyaltyRewards => Set<LoyaltyReward>();
    public DbSet<LoyaltyRedemption> LoyaltyRedemptions => Set<LoyaltyRedemption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("loyalty");

        modelBuilder.Entity<LoyaltyProgram>(e =>
        {
            e.ToTable("loyalty_programs");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Name).HasColumnName("name").IsRequired();
            e.Property(p => p.PointsPerEuro).HasColumnName("points_per_euro")
                .HasColumnType("numeric(10,4)").HasDefaultValue(10m);
            e.Property(p => p.EuroPerPoint).HasColumnName("euro_per_point")
                .HasColumnType("numeric(10,6)").HasDefaultValue(0.01m);
            e.Property(p => p.ExpiryDays).HasColumnName("expiry_days");
            e.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<LoyaltyTier>(e =>
        {
            e.ToTable("loyalty_tiers");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasColumnName("id");
            e.Property(t => t.ProgramId).HasColumnName("program_id").IsRequired();
            e.Property(t => t.Name).HasColumnName("name").IsRequired();
            e.Property(t => t.MinPoints).HasColumnName("min_points").HasDefaultValue(0);
            e.Property(t => t.BonusMultiplier).HasColumnName("bonus_multiplier")
                .HasColumnType("numeric(5,2)").HasDefaultValue(1.0m);
            e.Property(t => t.Perks).HasColumnName("perks")
                .HasColumnType("jsonb").HasDefaultValueSql("'{}'");
            e.Property(t => t.Rank).HasColumnName("rank").HasDefaultValue(1);
            e.HasIndex(t => new { t.ProgramId, t.Name }).IsUnique();
            e.HasIndex(t => new { t.ProgramId, t.Rank }).IsUnique();
        });

        modelBuilder.Entity<LoyaltyAccount>(e =>
        {
            e.ToTable("loyalty_accounts");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasColumnName("id");
            e.Property(a => a.UserId).HasColumnName("user_id").IsRequired();
            e.Property(a => a.ProgramId).HasColumnName("program_id").IsRequired();
            e.Property(a => a.CurrentTierId).HasColumnName("current_tier_id");
            e.Property(a => a.PointsBalance).HasColumnName("points_balance").HasDefaultValue(0);
            e.Property(a => a.PointsLifetime).HasColumnName("points_lifetime").HasDefaultValue(0);
            e.Property(a => a.TierUpdatedAt).HasColumnName("tier_updated_at");
            e.Property(a => a.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            e.HasIndex(a => new { a.UserId, a.ProgramId }).IsUnique();

            e.HasOne(a => a.CurrentTier)
                .WithMany()
                .HasForeignKey(a => a.CurrentTierId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasMany(a => a.Transactions)
                .WithOne()
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoyaltyTransaction>(e =>
        {
            e.ToTable("loyalty_transactions");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasColumnName("id");
            e.Property(t => t.AccountId).HasColumnName("account_id").IsRequired();
            e.Property(t => t.OrderId).HasColumnName("order_id");
            e.Property(t => t.RewardId).HasColumnName("reward_id");
            e.Property(t => t.Type).HasColumnName("type").IsRequired();
            e.Property(t => t.Points).HasColumnName("points").IsRequired();
            e.Property(t => t.Description).HasColumnName("description");
            e.Property(t => t.ExpiresAt).HasColumnName("expires_at");
            e.Property(t => t.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<LoyaltyReward>(e =>
        {
            e.ToTable("loyalty_rewards");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasColumnName("id");
            e.Property(r => r.ProgramId).HasColumnName("program_id").IsRequired();
            e.Property(r => r.Name).HasColumnName("name").IsRequired();
            e.Property(r => r.RewardType).HasColumnName("reward_type").IsRequired();
            e.Property(r => r.PointsCost).HasColumnName("points_cost").IsRequired();
            e.Property(r => r.DiscountValue).HasColumnName("discount_value")
                .HasColumnType("numeric(10,2)");
            e.Property(r => r.Stock).HasColumnName("stock");
            e.Property(r => r.Metadata).HasColumnName("metadata")
                .HasColumnType("jsonb").HasDefaultValueSql("'{}'");
            e.Property(r => r.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(r => r.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<LoyaltyRedemption>(e =>
        {
            e.ToTable("loyalty_redemptions");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasColumnName("id");
            e.Property(r => r.AccountId).HasColumnName("account_id").IsRequired();
            e.Property(r => r.RewardId).HasColumnName("reward_id").IsRequired();
            e.Property(r => r.OrderId).HasColumnName("order_id");
            e.Property(r => r.PointsSpent).HasColumnName("points_spent").IsRequired();
            e.Property(r => r.Status).HasColumnName("status").HasDefaultValue("pending").IsRequired();
            e.Property(r => r.RedeemedAt).HasColumnName("redeemed_at").HasDefaultValueSql("NOW()");
        });
    }
}
