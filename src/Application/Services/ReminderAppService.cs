using Application.DTOs.Reminders;
using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;

namespace Application.Services;

/// <summary>
/// Application service for payment reminder logic.
/// Identifies unpaid active subscriptions approaching their due dates
/// and generates notification DTOs (simulating Email/SMS).
/// </summary>
public class ReminderAppService : IReminderAppService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDebtCheckingService _debtCheckingService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<ReminderAppService> _logger;

    public ReminderAppService(
        ISubscriptionRepository subscriptionRepository,
        IPaymentRepository paymentRepository,
        IDebtCheckingService debtCheckingService,
        IDateTimeProvider dateTimeProvider,
        ILogger<ReminderAppService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _paymentRepository = paymentRepository;
        _debtCheckingService = debtCheckingService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<List<ReminderNotificationDto>> GetPendingRemindersAsync(
        int approachingDaysThreshold = 5, CancellationToken cancellationToken = default)
    {
        var reminders = new List<ReminderNotificationDto>();
        var now = _dateTimeProvider.UtcNow;
        var currentPeriod = $"{now.Year}_{now.Month:D2}";

        _logger.LogInformation(
            "Checking pending reminders. Period: {Period}, Threshold: {Threshold} days",
            currentPeriod, approachingDaysThreshold);

        // 1. Fetch all active subscriptions (with Customer navigation)
        var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync(cancellationToken);

        foreach (var subscription in activeSubscriptions)
        {
            try
            {
                // 2. Check if a successful payment exists for current period, SKIP if PAID
                var isPaid = await _paymentRepository.ExistsSuccessfulPaymentAsync(
                    subscription.Id, currentPeriod, cancellationToken);

                if (isPaid)
                {
                    _logger.LogDebug(
                        "Subscription {SubscriptionId} already paid for period {Period}. Skipping.",
                        subscription.Id, currentPeriod);
                    continue;
                }

                // 3. Call external debt checking service for unpaid subscriptions
                var debtResult = await Policy
                    .Handle<Exception>() // Any C# error
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(100 * retryAttempt)) // 3 times, with increasing intervals
                    .ExecuteAsync(async () => 
                    {
                        // Actual call is made inside the shield
                        return await _debtCheckingService.CheckDebtAsync(
                            subscription.SubscriptionNumber,
                            subscription.SubscriptionType.ToString(),
                            subscription.CurrentDebtAmount,
                            subscription.NextDueDate);
                    });

                // 4. Check if due date is approaching within threshold
                var daysUntilDue = (debtResult.DueDate - now).TotalDays;

                // Only process if due date is today (0) or in the future
                if (daysUntilDue >= 0 && daysUntilDue <= approachingDaysThreshold)
                {
                    var reminder = new ReminderNotificationDto
                    {
                        SubscriptionId = subscription.Id,
                        CustomerId = subscription.CustomerId,
                        CustomerFullName = subscription.Customer != null
                            ? $"{subscription.Customer.FirstName} {subscription.Customer.LastName}"
                            : "Unknown",
                        CustomerEmail = subscription.Customer?.Email ?? string.Empty,
                        SubscriptionTypeName = subscription.SubscriptionType.ToString(),
                        ServiceProviderName = subscription.ServiceProviderName,
                        SubscriptionNumber = subscription.SubscriptionNumber,
                        DebtAmount = debtResult.DebtAmount,
                        DueDate = debtResult.DueDate,
                        DaysUntilDue = (int)Math.Ceiling(daysUntilDue),
                        Period = debtResult.Period,
                        NotificationMessage = GenerateNotificationMessage(
                            subscription.Customer != null
                                ? $"{subscription.Customer.FirstName} {subscription.Customer.LastName}"
                                : "Valued Customer",
                            subscription.SubscriptionType.ToString(),
                            subscription.ServiceProviderName,
                            debtResult.DebtAmount,
                            debtResult.DueDate,
                            (int)Math.Ceiling(daysUntilDue))
                    };

                    reminders.Add(reminder);

                    _logger.LogInformation(
                        "Reminder generated. SubscriptionId: {SubscriptionId}, DebtAmount: {Amount:C}, DaysUntilDue: {Days}",
                        subscription.Id, debtResult.DebtAmount, (int)Math.Ceiling(daysUntilDue));
                }
            }
            catch (Exception ex)
            {
                // Log and continue — one failed debt check should not block other reminders
                _logger.LogWarning(ex,
                    "Failed to check debt for Subscription {SubscriptionId}. Skipping reminder.",
                    subscription.Id);
            }
        }

        _logger.LogInformation("Reminder check completed. Total reminders generated: {Count}", reminders.Count);
        return reminders;
    }

    /// <summary>
    /// Generates a simulated Email/SMS notification message.
    /// </summary>
    private static string GenerateNotificationMessage(
        string customerName, string subscriptionType, string providerName,
        decimal debtAmount, DateTime dueDate, int daysUntilDue)
    {
        return $"Sayın {customerName}, " +
               $"{providerName} {subscriptionType} aboneliğinizin borcu bulunmaktadır. " +
               $"Borç tutarı {debtAmount:F2} TL'dir. " + 
               $"Ödeme tarihi {dueDate:dd/MM/yyyy} ({daysUntilDue} gün kaldı). " +
               $"Ödemenizi yaparak hizmet kesintisini önleyebilirsiniz.";
    }
}
