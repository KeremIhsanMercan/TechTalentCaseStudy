namespace Domain.Entities;

/// <summary>
/// Base entity providing common audit properties for all domain entities.
/// CreatedDate and UpdatedDate are populated automatically via DbContext SaveChanges interception.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public bool IsDeleted { get; set; } = false;
}
