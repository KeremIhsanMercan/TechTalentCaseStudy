using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of ISubscriptionRepository.
/// Soft-delete filtering is handled transparently by AppDbContext global query filters.
/// </summary>
public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _context;

    public SubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Subscription?> GetByIdWithCustomerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<List<Subscription>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(s => s.Customer)
            .OrderBy(s => s.ServiceProviderName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Subscription>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(s => s.Customer)
            .Where(s => s.CustomerId == customerId)
            .OrderBy(s => s.SubscriptionType)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Subscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(s => s.Customer)
            .Where(s => s.IsActive)
            .OrderBy(s => s.ServiceProviderName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        await _context.Subscriptions.AddAsync(subscription, cancellationToken);
    }

    public void Update(Subscription subscription)
    {
        _context.Subscriptions.Update(subscription);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions.AnyAsync(s => s.Id == id, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
