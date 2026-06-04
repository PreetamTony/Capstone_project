namespace HospitalManagement.DataAccess.Models;

/// <summary>
/// Base entity with common audit fields. All entities inherit from this.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Unique identifier (GUID primary key).</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>UTC timestamp when the record was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the last update.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Soft-delete flag. Records with this set to true are excluded from normal queries.</summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>UTC timestamp when the record was soft-deleted.</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>ID of the user who deleted the record.</summary>
    public Guid? DeletedBy { get; set; }
}
