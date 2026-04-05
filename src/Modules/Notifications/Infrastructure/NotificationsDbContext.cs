using Ecommerce.Notifications.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Notifications.Infrastructure;

public class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
        : base(options) { }

    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<UserNotificationPreference> UserNotificationPreferences => Set<UserNotificationPreference>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notifications");

        modelBuilder.Entity<NotificationTemplate>(e =>
        {
            e.ToTable("notification_templates");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasColumnName("id");
            e.Property(t => t.EventType).HasColumnName("event_type").IsRequired();
            e.Property(t => t.Channel).HasColumnName("channel").IsRequired();
            e.Property(t => t.Lang).HasColumnName("lang").HasDefaultValue("fr").IsRequired();
            e.Property(t => t.Subject).HasColumnName("subject");
            e.Property(t => t.BodyTemplate).HasColumnName("body_template").IsRequired();
            e.Property(t => t.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.HasIndex(t => new { t.EventType, t.Channel, t.Lang }).IsUnique();
        });

        modelBuilder.Entity<UserNotificationPreference>(e =>
        {
            e.ToTable("user_notification_preferences");
            e.HasKey(p => new { p.UserId, p.Channel, p.EventType });
            e.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
            e.Property(p => p.Channel).HasColumnName("channel").IsRequired();
            e.Property(p => p.EventType).HasColumnName("event_type").IsRequired();
            e.Property(p => p.Subscribed).HasColumnName("subscribed").HasDefaultValue(true);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("notifications");
            e.HasKey(n => n.Id);
            e.Property(n => n.Id).HasColumnName("id");
            e.Property(n => n.UserId).HasColumnName("user_id").IsRequired();
            e.Property(n => n.TemplateId).HasColumnName("template_id");
            e.Property(n => n.OrderId).HasColumnName("order_id");
            e.Property(n => n.Channel).HasColumnName("channel").IsRequired();
            e.Property(n => n.Status).HasColumnName("status").HasDefaultValue("pending").IsRequired();
            e.Property(n => n.Payload).HasColumnName("payload")
                .HasColumnType("jsonb").HasDefaultValueSql("'{}'");
            e.Property(n => n.ScheduledAt).HasColumnName("scheduled_at")
                .HasDefaultValueSql("NOW()");
            e.Property(n => n.SentAt).HasColumnName("sent_at");
            e.Property(n => n.CreatedAt).HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            e.HasOne(n => n.Template)
                .WithMany()
                .HasForeignKey(n => n.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
