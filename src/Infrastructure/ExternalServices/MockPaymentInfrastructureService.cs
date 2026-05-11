using System.Net;
using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ExternalServices;

/// <summary>
/// Mock implementation of the third-party payment processing REST service.
/// Simulates realistic network behavior including latency and transient failures (15% failure rate)
/// to demonstrate Polly resilience policies.
/// </summary>
public class MockPaymentInfrastructureService : IPaymentInfrastructureService
{
    private readonly HttpClient _httpClient;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<MockPaymentInfrastructureService> _logger;
    private static readonly Random _random = new();

    public MockPaymentInfrastructureService(
        HttpClient httpClient,
        IDateTimeProvider dateTimeProvider,
        ILogger<MockPaymentInfrastructureService> logger)
    {
        _httpClient = httpClient;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<PaymentResultDto> ProcessPaymentAsync(string subscriptionNumber, decimal amount)
    {
        _logger.LogInformation(
            "Payment processing initiated. SubscriptionNumber: {SubscriptionNumber}, Amount: {Amount:C}",
            subscriptionNumber, amount);

        // Simulate realistic network latency (100-150ms)
        var latencyMs = _random.Next(100, 150);
        await Task.Delay(latencyMs);

        // Simulate transient failures 5% of the time
        SimulateTransientFailure(subscriptionNumber, amount);

        // Payment succeeds after passing resilience checks
        var result = new PaymentResultDto
        {
            IsSuccessful = true,
            TransactionId = GenerateTransactionId(),
            ProcessedAt = _dateTimeProvider.UtcNow
        };

        _logger.LogInformation(
            "Payment processed successfully. SubscriptionNumber: {SubscriptionNumber}, Amount: {Amount:C}, TransactionId: {TransactionId}",
            subscriptionNumber, amount, result.TransactionId);

        return result;
    }

    /// <summary>
    /// Simulates transient network failures (HTTP 503 or HttpRequestException) with a 5% probability.
    /// This enables active demonstration of the Polly retry and circuit breaker policies.
    /// </summary>
    private void SimulateTransientFailure(string subscriptionNumber, decimal amount)
    {
        var failureRoll = _random.Next(1, 101); // 1-100

        if (failureRoll <= 5)
        {
            if (failureRoll <= 2)
            {
                _logger.LogWarning(
                    "SIMULATED FAULT: HttpRequestException for payment. SubscriptionNumber: {SubscriptionNumber}, Amount: {Amount:C}",
                    subscriptionNumber, amount);
                throw new HttpRequestException(
                    "Simulated network failure: Payment Service is unreachable.",
                    null,
                    HttpStatusCode.ServiceUnavailable);
            }
            else
            {
                _logger.LogWarning(
                    "SIMULATED FAULT: HTTP 503 response for payment. SubscriptionNumber: {SubscriptionNumber}, Amount: {Amount:C}",
                    subscriptionNumber, amount);
                throw new HttpRequestException(
                    "Simulated HTTP 503: Payment Service temporarily unavailable.",
                    null,
                    HttpStatusCode.ServiceUnavailable);
            }
        }
    }

    /// <summary>
    /// Generates a unique transaction ID in bank-style format.
    /// </summary>
    private string GenerateTransactionId()
    {
        return $"TXN-{_dateTimeProvider.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
    }
}
