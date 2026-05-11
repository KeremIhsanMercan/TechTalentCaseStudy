namespace Application.DTOs.Reminders;

/// <summary>
/// DTO representing a pending payment reminder notification.
/// Simulates Email/SMS content for approaching due dates.
/// </summary>
public class ReminderNotificationDto
{
    public Guid SubscriptionId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerFullName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string SubscriptionTypeName { get; set; } = string.Empty;
    public string ServiceProviderName { get; set; } = string.Empty;
    public string SubscriptionNumber { get; set; } = string.Empty;
    public decimal DebtAmount { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysUntilDue { get; set; }
    public string Period { get; set; } = string.Empty;

    /// <summary>
    /// Simulated notification message content (Email/SMS body).
    /// </summary>
    public string NotificationMessage { get; set; } = string.Empty;
}
