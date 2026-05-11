using Application.DTOs.Reminders;

namespace Application.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(ReminderNotificationDto reminder, CancellationToken cancellationToken = default);
}
