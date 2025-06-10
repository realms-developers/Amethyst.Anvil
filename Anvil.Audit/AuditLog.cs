using Amethyst.Storages.Mongo;
using MongoDB.Bson.Serialization.Attributes;

namespace Anvil.Audit;

[BsonIgnoreExtraElements]
public sealed class AuditLog : DataModel
{
    public AuditLog(string name) : base(name)
    {
    }

    public string Server { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = string.Empty;
    public string? User { get; set; }
    public Dictionary<string, string> Objects { get; set; } = new();
    public string[] Tags { get; set; } = Array.Empty<string>();

    public override void Save()
    {
        throw new InvalidOperationException("Audit logs are not saved directly. They are managed by the audit module.");
    }
    public override void Remove()
    {
        throw new InvalidOperationException("Audit logs are not removed directly. They are managed by the audit module.");
    }
}