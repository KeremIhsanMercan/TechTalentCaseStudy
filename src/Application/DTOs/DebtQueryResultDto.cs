namespace Application.DTOs;

/// <summary>
/// Represents the result of a debt inquiry against a third-party debt checking service.
/// </summary>
public class DebtQueryResultDto
{
    /// <summary>
    /// Outstanding debt amount for the subscription.
    /// </summary>
    public decimal DebtAmount { get; set; }

    /// <summary>
    /// Due date for the current billing period.
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Billing period in "YYYY_MM" format (e.g., "2026_05").
    /// </summary>
    public string Period { get; set; } = string.Empty;
}
