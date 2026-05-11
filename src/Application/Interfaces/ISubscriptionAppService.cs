using Application.DTOs.Subscriptions;

namespace Application.Interfaces;

/// <summary>
/// Application service contract for Subscription CRUD operations.
/// </summary>
public interface ISubscriptionAppService
{
    Task<SubscriptionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SubscriptionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<SubscriptionDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<SubscriptionDto> CreateAsync(CreateSubscriptionDto dto, CancellationToken cancellationToken = default);
    Task<SubscriptionDto> UpdateAsync(UpdateSubscriptionDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
