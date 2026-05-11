using Domain.Entities;

namespace Domain.Interfaces;

/// <summary>
/// Repository contract for Payment entity operations.
/// Includes idempotency-critical query for duplicate payment detection.
/// All queries respect soft-delete via global query filters.
/// </summary>
public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Payment>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a successful payment already exists for a given subscription and period.
    /// Critical for idempotency enforcement in financial operations.
    /// </summary>
    Task<bool> ExistsSuccessfulPaymentAsync(Guid subscriptionId, string period, CancellationToken cancellationToken = default);

    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
