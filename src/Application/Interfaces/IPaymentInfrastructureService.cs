using Application.DTOs;

namespace Application.Interfaces;

/// <summary>
/// Contract for the third-party payment processing service.
/// Sends payment requests to the external provider for subscription bill payments.
/// </summary>
public interface IPaymentInfrastructureService
{
    /// <summary>
    /// Processes a payment through the external payment provider.
    /// </summary>
    /// <param name="subscriptionNumber">The unique subscription identifier at the service provider.</param>
    /// <param name="amount">The payment amount to process.</param>
    /// <returns>Payment result indicating success/failure, transaction ID, and processing timestamp.</returns>
    Task<PaymentResultDto> ProcessPaymentAsync(string subscriptionNumber, decimal amount);
}
