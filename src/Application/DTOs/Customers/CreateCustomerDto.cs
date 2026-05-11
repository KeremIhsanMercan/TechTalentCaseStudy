namespace Application.DTOs.Customers;

/// <summary>
/// DTO for creating a new customer.
/// Customer update is excluded per business rules.
/// </summary>
public class CreateCustomerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
