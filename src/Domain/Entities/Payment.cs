namespace Domain.Entities;

/// <summary>
/// Represents a payment record for a subscription.
/// Payment records must never be physically deleted (financial audit requirement).
/// </summary>
public class Payment : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }

    /// <summary>
    /// Payment period in "YYYY_MM" format (e.g., "2026_05").
    /// </summary>
    public string Period { get; set; } = string.Empty;

    public bool IsSuccessful { get; set; }

    // Navigation property
    public Subscription Subscription { get; set; } = null!;
}
