namespace Domain.Exceptions;

/// <summary>
/// Thrown when a requested subscription cannot be found in the system.
/// </summary>
public class SubscriptionNotFoundException : Exception
{
    public Guid SubscriptionId { get; }

    public SubscriptionNotFoundException(Guid subscriptionId)
        : base($"Subscription with ID '{subscriptionId}' was not found.")
    {
        SubscriptionId = subscriptionId;
    }

    public SubscriptionNotFoundException(Guid subscriptionId, string message)
        : base(message)
    {
        SubscriptionId = subscriptionId;
    }
}
