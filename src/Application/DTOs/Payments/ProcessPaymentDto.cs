namespace Application.DTOs.Payments;

/// <summary>
/// DTO for initiating a payment processing request.
/// </summary>
public class ProcessPaymentDto
{
    public Guid SubscriptionId { get; set; }
    public decimal Amount { get; set; }
}
