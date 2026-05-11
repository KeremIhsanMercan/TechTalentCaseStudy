namespace Domain.Exceptions;

/// <summary>
/// Thrown when a requested customer cannot be found in the system.
/// </summary>
public class CustomerNotFoundException : Exception
{
    public Guid CustomerId { get; }

    public CustomerNotFoundException(Guid customerId)
        : base($"Customer with ID '{customerId}' was not found.")
    {
        CustomerId = customerId;
    }

    public CustomerNotFoundException(Guid customerId, string message)
        : base(message)
    {
        CustomerId = customerId;
    }
}
