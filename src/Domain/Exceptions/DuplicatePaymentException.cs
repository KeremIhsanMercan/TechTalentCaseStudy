namespace Domain.Exceptions;

/// <summary>
/// Thrown when a duplicate payment is attempted for a subscription period that already has a successful payment.
/// Critical for idempotency enforcement in financial operations.
/// </summary>
public class DuplicatePaymentException : Exception
{
    public Guid SubscriptionId { get; }
    public string Period { get; }

    public DuplicatePaymentException(Guid subscriptionId, string period)
        : base($"A successful payment for subscription '{subscriptionId}' in period '{period}' already exists. Duplicate payments are not allowed.")
    {
        SubscriptionId = subscriptionId;
        Period = period;
    }
}
