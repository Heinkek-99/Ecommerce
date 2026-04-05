using Ecommerce.Notifications.Application.Jobs;
using Ecommerce.Notifications.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Notifications.Extensions;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<NotificationsDbContext>(opts =>
            opts.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<EmailSender>();
        services.AddScoped<SmsSender>();
        services.AddScoped<ProcessPendingNotificationsJob>();

        return services;
    }
}
