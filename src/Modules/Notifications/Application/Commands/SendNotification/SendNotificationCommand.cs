using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Notifications.Application.Commands.SendNotification;

public record SendNotificationCommand(
    Guid UserId,
    string Channel,
    string TemplateCode,
    Dictionary<string, string> Variables,
    Guid? OrderId = null) : ICommand<Result>;
