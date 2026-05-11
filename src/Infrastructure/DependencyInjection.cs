using Application.Interfaces;
using Domain.Interfaces;
using Infrastructure.ExternalServices;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection registration.
/// Configures DbContext, time abstraction, typed HTTP clients with Polly resilience policies.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //  Time Abstraction 
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        //  EF Core DbContext 
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        //  Repositories 
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        //  Polly Resilience Policies 
        // Retry Policy: 3 retries with exponential backoff for transient HTTP errors
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError() // HttpRequestException, HTTP 5xx, HTTP 408
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Logging is handled via Polly context; structured logging in production
                    context.GetLogger()?.LogWarning(
                        "Polly Retry #{RetryAttempt} after {Delay}s. Reason: {Reason}",
                        retryAttempt,
                        timespan.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });

        //  Typed HTTP Client: Debt Checking Service 
        services.AddHttpClient<IDebtCheckingService, MockDebtCheckingService>(client =>
            {
                client.BaseAddress = new Uri("https://mock-debt-api.example.com/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(4, TimeSpan.FromSeconds(30))); // Specific Circuit Breaker for Debt Checking Service

        //  Typed HTTP Client: Payment Infrastructure Service 
        services.AddHttpClient<IPaymentInfrastructureService, MockPaymentInfrastructureService>(client =>
            {
                client.BaseAddress = new Uri("https://mock-payment-api.example.com/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(4, TimeSpan.FromSeconds(30))); // Specific Circuit Breaker for Payment Infrastructure Service

        return services;
    }
}

/// <summary>
/// Extension to extract a logger from Polly context for structured retry logging.
/// </summary>
internal static class PollyContextExtensions
{
    private static readonly string LoggerKey = "ILogger";

    public static Microsoft.Extensions.Logging.ILogger? GetLogger(this Context context)
    {
        if (context.TryGetValue(LoggerKey, out var logger))
        {
            return logger as Microsoft.Extensions.Logging.ILogger;
        }
        return null;
    }
}
