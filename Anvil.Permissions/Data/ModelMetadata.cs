namespace Anvil.Permissions.Data;

public record ModelMetadata(string createdBy, DateTime createdAt, string? updatedBy = null, DateTime? updatedAt = null)
{
    public ModelMetadata() : this(string.Empty, DateTime.UtcNow) { }

    public ModelMetadata(string createdBy) : this(createdBy, DateTime.UtcNow) { }

    public ModelMetadata(string createdBy, DateTime createdAt) : this(createdBy, createdAt, null, null) { }
}