namespace Application.DTOs.Reminders;

public class ReminderSettings
{
    public int NotificationThresholdDays { get; set; } = 5; // Fallback value
}