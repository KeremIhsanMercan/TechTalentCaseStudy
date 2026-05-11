using Application.DTOs;
using Application.DTOs.Payments;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentAppService _paymentAppService;
    private readonly ISummaryAppService _summaryAppService;

    public PaymentsController(IPaymentAppService paymentAppService, ISummaryAppService summaryAppService)
    {
        _paymentAppService = paymentAppService;
        _summaryAppService = summaryAppService;
    }

    [HttpPost("process")]
    [ProducesResponseType(typeof(PaymentResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] // Idempotency violation
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto, CancellationToken cancellationToken)
    {
        var result = await _paymentAppService.ProcessSubscriptionPaymentAsync(dto.SubscriptionId, dto.Amount, cancellationToken);
        return Ok(result);
    }

    [HttpGet("subscription/{subscriptionId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionPaymentHistory(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var history = await _summaryAppService.GetSubscriptionPaymentHistoryAsync(subscriptionId, cancellationToken);
        return Ok(history.FirstOrDefault()?.Payments ?? new List<PaymentHistoryDto>());
    }
}
