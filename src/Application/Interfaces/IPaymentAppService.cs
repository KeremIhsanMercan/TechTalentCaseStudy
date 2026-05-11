using Application.DTOs;

namespace Application.Interfaces;

/// <summary>
/// Application service contract for payment processing with idempotency enforcement.
/// Critical financial service — ensures no duplicate payments per period.
/// </summary>
public interface IPaymentAppService
{
    /// <summary>
    /// Processes a payment for a subscription with strict idempotency enforcement.
    /// Checks for existing successful payments in the current period before processing.
    /// </summary>
    Task<PaymentResultDto> ProcessSubscriptionPaymentAsync(Guid subscriptionId, decimal amount, CancellationToken cancellationToken = default);
}
