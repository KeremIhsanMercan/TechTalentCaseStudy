using Application.DTOs.Customers;

namespace Application.Interfaces;

/// <summary>
/// Application service contract for Customer CRUD operations.
/// Customer update is excluded per business rules.
/// </summary>
public interface ICustomerAppService
{
    Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
