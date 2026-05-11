using Application.DTOs.Reminders;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RemindersController : ControllerBase
{
    private readonly IReminderAppService _reminderAppService;

    public RemindersController(IReminderAppService reminderAppService)
    {
        _reminderAppService = reminderAppService;
    }

    [HttpGet("pending")]
    [ProducesResponseType(typeof(List<ReminderNotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingReminders([FromQuery] int thresholdDays = 5, CancellationToken cancellationToken = default)
    {
        var reminders = await _reminderAppService.GetPendingRemindersAsync(thresholdDays, cancellationToken);
        return Ok(reminders);
    }
}
