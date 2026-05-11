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
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<SubscriptionAppService> _logger;

    public SubscriptionAppService(
        ISubscriptionRepository subscriptionRepository,
        ICustomerRepository customerRepository,
        IValidator<CreateSubscriptionDto> createValidator,
        IValidator<UpdateSubscriptionDto> updateValidator,
        IDateTimeProvider dateTimeProvider,
        ILogger<SubscriptionAppService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _customerRepository = customerRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _dateTimeProvider = dateTimeProvider;
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

        var now = _dateTimeProvider.UtcNow;
        var random = new Random();
        var initialDebt = GenerateInitialDebtAmount(dto.SubscriptionType.ToString(), random);
        var nextDueDate = now.AddDays(random.Next(2, 26));

        var subscription = new Subscription
        {
            CustomerId = dto.CustomerId,
            SubscriptionType = dto.SubscriptionType,
            ServiceProviderName = dto.ServiceProviderName,
            SubscriptionNumber = dto.SubscriptionNumber,
            IsActive = true, // New subscriptions are active by default
            CurrentDebtAmount = initialDebt,
            NextDueDate = nextDueDate
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
            CurrentDebtAmount = subscription.CurrentDebtAmount,
            NextDueDate = subscription.NextDueDate,
            CreatedDate = subscription.CreatedDate
        };
    }

    /// <summary>
    /// Generates a realistic initial debt amount based on subscription type.
    /// </summary>
    private static decimal GenerateInitialDebtAmount(string subscriptionType, Random random)
    {
        var (min, max) = subscriptionType?.ToUpper(new System.Globalization.CultureInfo("tr-TR")) switch
        {
            "ELEKTRİK"    => (150.00m, 850.00m),
            "SU"          => (50.00m, 300.00m),
            "DOĞALGAZ"    => (100.00m, 600.00m),
            "İNTERNET"    => (150.00m, 500.00m),
            "CEPTELEFONU" => (80.00m, 350.00m),
            "TELEVİZYON"  => (50.00m, 200.00m),
            "SİGORTA"     => (200.00m, 1500.00m),
            "KREDİKARTI"  => (500.00m, 5000.00m),
            "KİRA"        => (2000.00m, 15000.00m),
            "AİDAT"       => (100.00m, 1000.00m),
            _             => (50.00m, 500.00m)
        };

        var range = max - min;
        var randomFactor = (decimal)random.NextDouble();
        return Math.Round(min + range * randomFactor, 2);
    }
}
