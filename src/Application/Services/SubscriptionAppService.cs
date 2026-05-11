using Application.DTOs.Subscriptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
/// Application service for Subscription CRUD operations with soft-delete.
/// </summary>
public class SubscriptionAppService : ISubscriptionAppService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IValidator<CreateSubscriptionDto> _createValidator;
    private readonly IValidator<UpdateSubscriptionDto> _updateValidator;
    private readonly ILogger<SubscriptionAppService> _logger;

    public SubscriptionAppService(
        ISubscriptionRepository subscriptionRepository,
        ICustomerRepository customerRepository,
        IValidator<CreateSubscriptionDto> createValidator,
        IValidator<UpdateSubscriptionDto> updateValidator,
        ILogger<SubscriptionAppService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _customerRepository = customerRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<SubscriptionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdWithCustomerAsync(id, cancellationToken)
            ?? throw new SubscriptionNotFoundException(id);

        return MapToDto(subscription);
    }

    public async Task<List<SubscriptionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionRepository.GetAllAsync(cancellationToken);
        return subscriptions.Select(MapToDto).ToList();
    }

    public async Task<List<SubscriptionDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        // Verify customer exists
        if (!await _customerRepository.ExistsAsync(customerId, cancellationToken))
            throw new CustomerNotFoundException(customerId);

        var subscriptions = await _subscriptionRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return subscriptions.Select(MapToDto).ToList();
    }

    public async Task<SubscriptionDto> CreateAsync(CreateSubscriptionDto dto, CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Verify customer exists
        if (!await _customerRepository.ExistsAsync(dto.CustomerId, cancellationToken))
            throw new CustomerNotFoundException(dto.CustomerId);

        var subscription = new Subscription
        {
            CustomerId = dto.CustomerId,
            SubscriptionType = dto.SubscriptionType,
            ServiceProviderName = dto.ServiceProviderName,
            SubscriptionNumber = dto.SubscriptionNumber,
            IsActive = true // New subscriptions are active by default
        };

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _subscriptionRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Subscription created. Id: {SubscriptionId}, CustomerId: {CustomerId}, Type: {Type}",
            subscription.Id, subscription.CustomerId, subscription.SubscriptionType);

        // Reload with customer navigation for DTO mapping
        var created = await _subscriptionRepository.GetByIdWithCustomerAsync(subscription.Id, cancellationToken);
        return MapToDto(created!);
    }

    public async Task<SubscriptionDto> UpdateAsync(UpdateSubscriptionDto dto, CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var subscription = await _subscriptionRepository.GetByIdWithCustomerAsync(dto.Id, cancellationToken)
            ?? throw new SubscriptionNotFoundException(dto.Id);

        // Update mutable fields
        subscription.SubscriptionType = dto.SubscriptionType;
        subscription.ServiceProviderName = dto.ServiceProviderName;
        subscription.SubscriptionNumber = dto.SubscriptionNumber;
        subscription.IsActive = dto.IsActive;

        _subscriptionRepository.Update(subscription);
        await _subscriptionRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Subscription updated. Id: {SubscriptionId}", subscription.Id);

        return MapToDto(subscription);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new SubscriptionNotFoundException(id);

        // Soft delete
        subscription.IsDeleted = true;
        subscription.IsActive = false;
        _subscriptionRepository.Update(subscription);
        await _subscriptionRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Subscription soft-deleted. Id: {SubscriptionId}", id);
    }

    private static SubscriptionDto MapToDto(Subscription subscription)
    {
        return new SubscriptionDto
        {
            Id = subscription.Id,
            CustomerId = subscription.CustomerId,
            CustomerFullName = subscription.Customer != null
                ? $"{subscription.Customer.FirstName} {subscription.Customer.LastName}"
                : string.Empty,
            SubscriptionType = subscription.SubscriptionType,
            SubscriptionTypeName = subscription.SubscriptionType.ToString(),
            ServiceProviderName = subscription.ServiceProviderName,
            SubscriptionNumber = subscription.SubscriptionNumber,
            IsActive = subscription.IsActive,
            CreatedDate = subscription.CreatedDate
        };
    }
}
