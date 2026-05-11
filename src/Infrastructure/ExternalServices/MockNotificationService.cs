using Application.DTOs.Reminders;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ExternalServices;

/// <summary>
/// Mock implementation of the Notification Service
/// </summary>
public class MockNotificationService(ILogger<MockNotificationService> logger) : INotificationService
{
    public async Task SendNotificationAsync(ReminderNotificationDto reminder, CancellationToken cancellationToken = default)
    {
        // Realistic async network delay simulation
        await Task.Delay(50, cancellationToken);
        
        // Turkish log template
        logger.LogInformation(
            "[MOCK BİLDİRİM SERVİSİ] Alıcı: {Email} | Abonelik: {SubscriptionType} | Mesaj: {NotificationMessage}",
            reminder.CustomerEmail, 
            reminder.SubscriptionTypeName, 
            reminder.NotificationMessage);
    }
}