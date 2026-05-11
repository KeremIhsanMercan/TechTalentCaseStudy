using Domain.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Concrete implementation of IDateTimeProvider using system UTC clock.
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
