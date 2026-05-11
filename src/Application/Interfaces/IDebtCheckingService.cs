using Application.DTOs;

namespace Application.Interfaces;

/// <summary>
/// Contract for the third-party debt checking service.
/// Queries the external provider to determine current outstanding debt for a subscription.
/// </summary>
public interface IDebtCheckingService
{
    /// <summary>
    /// Checks the current outstanding debt for a given subscription.
    /// </summary>
    /// <param name="subscriptionNumber">The unique subscription identifier at the service provider.</param>
    /// <param name="subscriptionType">The type of subscription (e.g., "Electricity", "Water").</param>
    /// <returns>Debt query result containing amount, due date, and billing period.</returns>
    Task<DebtQueryResultDto> CheckDebtAsync(string subscriptionNumber, string subscriptionType);
}
