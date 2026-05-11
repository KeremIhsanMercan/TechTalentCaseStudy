using System.Net;
using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ExternalServices;

/// <summary>
/// Mock implementation of the third-party debt checking REST service.
/// Simulates realistic network behavior including latency and transient failures (15% failure rate)
/// to demonstrate Polly resilience policies.
/// </summary>
public class MockDebtCheckingService : IDebtCheckingService
{
    private readonly HttpClient _httpClient;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<MockDebtCheckingService> _logger;
    private static readonly Random _random = new();

    public MockDebtCheckingService(
        HttpClient httpClient,
        IDateTimeProvider dateTimeProvider,
        ILogger<MockDebtCheckingService> logger)
    {
        _httpClient = httpClient;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<DebtQueryResultDto> CheckDebtAsync(string subscriptionNumber, string subscriptionType, decimal currentDebtAmount, DateTime nextDueDate)
    {
        _logger.LogInformation(
            "Debt check initiated for SubscriptionNumber: {SubscriptionNumber}, Type: {SubscriptionType}",
            subscriptionNumber, subscriptionType);

        var now = _dateTimeProvider.UtcNow;
        var period = $"{now.Year}_{now.Month:D2}";

        // Simulate realistic network latency (100-150ms)
        var latencyMs = _random.Next(100, 150);
        await Task.Delay(latencyMs);

        // Simulate transient failures 5% of the time (non-deterministic using static _random)
        SimulateTransientFailure(subscriptionNumber);

        var result = new DebtQueryResultDto
        {
            DebtAmount = currentDebtAmount,
            DueDate = nextDueDate,
            Period = period
        };

        _logger.LogInformation(
            "Debt check completed for SubscriptionNumber: {SubscriptionNumber}. Amount: {Amount:C}, Period: {Period}",
            subscriptionNumber, result.DebtAmount, result.Period);

        return result;
    }

    /// <summary>
    /// Simulates transient network failures (HTTP 503 or HttpRequestException) with a 5% probability.
    /// This enables active demonstration of the Polly retry and circuit breaker policies.
    /// </summary>
    private void SimulateTransientFailure(string subscriptionNumber)
    {
        var failureRoll = _random.Next(1, 101); // 1-100

        if (failureRoll <= 5)
        {
            if (failureRoll <= 2)
            {
                _logger.LogWarning(
                    "SIMULATED FAULT: HttpRequestException for debt check. SubscriptionNumber: {SubscriptionNumber}",
                    subscriptionNumber);
                throw new HttpRequestException(
                    "Simulated network failure: Debt Checking Service is unreachable.",
                    null,
                    HttpStatusCode.ServiceUnavailable);
            }
            else
            {
                _logger.LogWarning(
                    "SIMULATED FAULT: HTTP 503 response for debt check. SubscriptionNumber: {SubscriptionNumber}",
                    subscriptionNumber);
                throw new HttpRequestException(
                    "Simulated HTTP 503: Debt Checking Service temporarily unavailable.",
                    null,
                    HttpStatusCode.ServiceUnavailable);
            }
        }
    }

}
