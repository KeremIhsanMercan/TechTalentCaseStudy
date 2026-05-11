namespace Domain.Entities;

/// <summary>
/// Represents a bank customer who can own multiple subscriptions.
/// </summary>
public class Customer : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    // Navigation property: A customer can have multiple subscriptions
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
