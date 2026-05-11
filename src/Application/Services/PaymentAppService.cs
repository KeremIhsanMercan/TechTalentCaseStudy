using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
/// Application service for payment processing with strict idempotency enforcement.
/// Critical financial service — ensures no duplicate payments per subscription/period.
/// </summary>
public class PaymentAppService : IPaymentAppService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaymentInfrastructureService _paymentInfrastructureService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<PaymentAppService> _logger;

    public PaymentAppService(
        IPaymentRepository paymentRepository,
        ISubscriptionRepository subscriptionRepository,
        IPaymentInfrastructureService paymentInfrastructureService,
        IDateTimeProvider dateTimeProvider,
        ILogger<PaymentAppService> logger)
    {
        _paymentRepository = paymentRepository;
        _subscriptionRepository = subscriptionRepository;
        _paymentInfrastructureService = paymentInfrastructureService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<PaymentResultDto> ProcessSubscriptionPaymentAsync(
        Guid subscriptionId, decimal amount, CancellationToken cancellationToken = default)
    {
        // 1. Verify subscription exists and is active
        var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken)
            ?? throw new SubscriptionNotFoundException(subscriptionId);

        if (!subscription.IsActive)
            throw new SubscriptionNotFoundException(subscriptionId,
                $"Subscription '{subscriptionId}' is not active. Cannot process payment.");

        // 2. IDEMPOTENCY CHECK: Determine current period and check for existing successful payment
        var currentPeriod = $"{_dateTimeProvider.UtcNow.Year}_{_dateTimeProvider.UtcNow.Month:D2}";
        var alreadyPaid = await _paymentRepository.ExistsSuccessfulPaymentAsync(
            subscriptionId, currentPeriod, cancellationToken);

        if (alreadyPaid)
        {
            _logger.LogWarning(
                "IDEMPOTENCY BLOCK: Duplicate payment attempt. SubscriptionId: {SubscriptionId}, Period: {Period}",
                subscriptionId, currentPeriod);
            throw new DuplicatePaymentException(subscriptionId, currentPeriod);
        }

        // 3. Call external payment service
        _logger.LogInformation(
            "Processing payment. SubscriptionId: {SubscriptionId}, Amount: {Amount:C}, Period: {Period}",
            subscriptionId, amount, currentPeriod);

        PaymentResultDto? externalResult = null;
        bool isSuccessful = false;

        try
        {
            externalResult = await _paymentInfrastructureService.ProcessPaymentAsync(
                subscription.SubscriptionNumber, amount);
            isSuccessful = externalResult.IsSuccessful;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "External payment service failed. SubscriptionId: {SubscriptionId}, Amount: {Amount:C}",
                subscriptionId, amount);
            // Payment failed — record as unsuccessful
            isSuccessful = false;
        }

        // 4. Persist payment record regardless of outcome (financial audit trail)
        var payment = new Payment
        {
            SubscriptionId = subscriptionId,
            Amount = amount,
            PaymentDate = _dateTimeProvider.UtcNow,
            Period = currentPeriod,
            IsSuccessful = isSuccessful
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment recorded. PaymentId: {PaymentId}, SubscriptionId: {SubscriptionId}, Success: {IsSuccessful}, Period: {Period}",
            payment.Id, subscriptionId, isSuccessful, currentPeriod);

        // 5. Return result
        return new PaymentResultDto
        {
            IsSuccessful = isSuccessful,
            TransactionId = externalResult?.TransactionId ?? $"FAILED-{payment.Id}",
            ProcessedAt = _dateTimeProvider.UtcNow
        };
    }
}
