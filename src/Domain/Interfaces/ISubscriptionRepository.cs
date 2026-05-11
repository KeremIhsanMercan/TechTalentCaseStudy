using Domain.Entities;

namespace Domain.Interfaces;

/// <summary>
/// Repository contract for Subscription entity operations.
/// All queries respect soft-delete via global query filters unless explicitly bypassed.
/// </summary>
public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByIdWithCustomerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Subscription>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Subscription>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<Subscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
    void Update(Subscription subscription);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
