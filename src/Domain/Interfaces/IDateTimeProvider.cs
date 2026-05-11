namespace Domain.Interfaces;

/// <summary>
/// Abstraction for date/time operations to ensure testability.
/// NEVER use DateTime.Now or DateTime.UtcNow directly in the application.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
