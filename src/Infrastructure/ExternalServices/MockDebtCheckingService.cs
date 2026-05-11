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

    public async Task<DebtQueryResultDto> CheckDebtAsync(string subscriptionNumber, string subscriptionType)
    {
        _logger.LogInformation(
            "Debt check initiated for SubscriptionNumber: {SubscriptionNumber}, Type: {SubscriptionType}",
            subscriptionNumber, subscriptionType);

        // Simulate realistic network latency (100-500ms)
        var latencyMs = _random.Next(100, 500);
        await Task.Delay(latencyMs);

        // Simulate transient failures 15% of the time
        SimulateTransientFailure(subscriptionNumber);

        // Generate a realistic debt amount based on subscription type
        var debtAmount = GenerateDebtAmount(subscriptionType);
        var now = _dateTimeProvider.UtcNow;
        var period = $"{now.Year}_{now.Month:D2}";
        var dueDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 0, 0, 0, DateTimeKind.Utc);

        var result = new DebtQueryResultDto
        {
            DebtAmount = debtAmount,
            DueDate = dueDate,
            Period = period
        };

        _logger.LogInformation(
            "Debt check completed for SubscriptionNumber: {SubscriptionNumber}. Amount: {Amount:C}, Period: {Period}",
            subscriptionNumber, result.DebtAmount, result.Period);

        return result;
    }

    /// <summary>
    /// Simulates transient network failures (HTTP 503 or HttpRequestException) with a 15% probability.
    /// This enables active demonstration of the Polly retry and circuit breaker policies.
    /// </summary>
    private void SimulateTransientFailure(string subscriptionNumber)
    {
        var failureRoll = _random.Next(1, 101); // 1-100

        if (failureRoll <= 15)
        {
            if (failureRoll <= 7)
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

    /// <summary>
    /// Generates a realistic debt amount based on subscription type.
    /// Different utility types have different typical price ranges.
    /// </summary>
    private static decimal GenerateDebtAmount(string subscriptionType)
    {
        var (min, max) = subscriptionType?.ToUpperInvariant() switch
        {
            "ELECTRICITY" => (150.00m, 850.00m),
            "WATER"       => (50.00m, 300.00m),
            "NATURALGAS"  => (100.00m, 600.00m),
            "INTERNET"    => (150.00m, 500.00m),
            "MOBILE"      => (80.00m, 350.00m),
            "TELEVISION"  => (50.00m, 200.00m),
            "INSURANCE"   => (200.00m, 1500.00m),
            _             => (50.00m, 500.00m)
        };

        // Generate amount with 2 decimal precision
        var range = max - min;
        var randomFactor = (decimal)_random.NextDouble();
        return Math.Round(min + range * randomFactor, 2);
    }
}
