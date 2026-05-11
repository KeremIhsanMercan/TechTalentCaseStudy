using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Background service that periodically triggers the payment reminder logic.
/// Runs independently of the REST API to guarantee isolated, scheduled execution.
/// </summary>
public class ReminderSchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReminderSchedulerService> _logger;

    public ReminderSchedulerService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReminderSchedulerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReminderSchedulerService is starting.");

        // Interval defined here as 24 Hours
        var period = TimeSpan.FromHours(24); 
        using var timer = new PeriodicTimer(period);

        try
        {
            _logger.LogInformation("ReminderSchedulerService initial execution triggered at: {time}", DateTimeOffset.Now);
            await TriggerReminderFlowAsync(stoppingToken);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation("ReminderSchedulerService periodic tick triggered at: {time}", DateTimeOffset.Now);
                await TriggerReminderFlowAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ReminderSchedulerService cancellation requested.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "A fatal error occurred in the ReminderSchedulerService execution loop.");
        }

        _logger.LogInformation("ReminderSchedulerService has stopped.");
    }

    private async Task TriggerReminderFlowAsync(CancellationToken stoppingToken)
    {
        // Create a scoped DI container manually since BackgroundService is Singleton
        using var scope = _scopeFactory.CreateScope();
        var reminderAppService = scope.ServiceProvider.GetRequiredService<IReminderAppService>();
        
        // Dispatch the notifications safely
        await reminderAppService.ProcessDailyNotificationsAsync(stoppingToken);
    }
}