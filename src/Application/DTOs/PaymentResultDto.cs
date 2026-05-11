namespace Application.DTOs;

/// <summary>
/// Represents the result of a payment processing request to the third-party payment service.
/// </summary>
public class PaymentResultDto
{
    /// <summary>
    /// Indicates whether the payment was successfully processed.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Unique transaction identifier returned by the payment provider.
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the payment was processed by the external service.
    /// </summary>
    public DateTime ProcessedAt { get; set; }
}
