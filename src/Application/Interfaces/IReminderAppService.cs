using Application.DTOs.Reminders;

namespace Application.Interfaces;

/// <summary>
/// Application service contract for payment reminder logic.
/// Identifies unpaid subscriptions approaching their due dates.
/// </summary>
public interface IReminderAppService
{
    /// <summary>
    /// Gets pending payment reminders for active subscriptions approaching their due date.
    /// Skips subscriptions already paid for the current period.
    /// Calls IDebtCheckingService to fetch real-time debt and due date information.
    /// </summary>
    /// <param name="approachingDaysThreshold">Number of days before due date to trigger a reminder (default: 5).</param>
    Task<List<ReminderNotificationDto>> GetPendingRemindersAsync(int approachingDaysThreshold = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Background job method to fetch pending reminders and dispatch notifications.
    /// Uses Partial Failure isolation to ensure robust execution.
    /// </summary>
    Task ProcessDailyNotificationsAsync(CancellationToken cancellationToken = default);
}
