namespace Application.DTOs.Payments;

/// <summary>
/// Read DTO for payment history records.
/// </summary>
public class PaymentHistoryDto
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public string SubscriptionNumber { get; set; } = string.Empty;
    public string ServiceProviderName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Period { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public DateTime CreatedDate { get; set; }
}
