using Application.DTOs.Subscriptions;
using Application.DTOs.Summary;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SummaryController : ControllerBase
{
    private readonly ISummaryAppService _summaryAppService;

    public SummaryController(ISummaryAppService summaryAppService)
    {
        _summaryAppService = summaryAppService;
    }

    [HttpGet("active-subscriptions")]
    [ProducesResponseType(typeof(List<SubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveSubscriptions(CancellationToken cancellationToken)
    {
        var result = await _summaryAppService.GetActiveSubscriptionsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("unpaid-subscriptions")]
    [ProducesResponseType(typeof(List<UnpaidSubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnpaidSubscriptionsForCurrentMonth(CancellationToken cancellationToken)
    {
        var result = await _summaryAppService.GetUnpaidSubscriptionsForCurrentMonthAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("payment-history")]
    [ProducesResponseType(typeof(List<SubscriptionPaymentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSubscriptionPaymentHistory(CancellationToken cancellationToken)
    {
        var result = await _summaryAppService.GetSubscriptionPaymentHistoryAsync(null, cancellationToken);
        return Ok(result);
    }
}
