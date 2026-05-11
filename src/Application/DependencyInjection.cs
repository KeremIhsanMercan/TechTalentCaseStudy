using Application.Interfaces;
using Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Application layer dependency injection registration.
/// Registers validators, application services, and related concerns.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // FluentValidation 
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Application Services 
        services.AddScoped<ICustomerAppService, CustomerAppService>();
        services.AddScoped<ISubscriptionAppService, SubscriptionAppService>();
        services.AddScoped<IPaymentAppService, PaymentAppService>();
        services.AddScoped<IReminderAppService, ReminderAppService>();
        services.AddScoped<ISummaryAppService, SummaryAppService>();

        return services;
    }
}
