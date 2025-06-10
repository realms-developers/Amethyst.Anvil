using Amethyst.Storages.Config;

namespace Anvil.Audit;

public sealed class AuditConfiguration
{
    static AuditConfiguration() => Configuration.Load();

    internal static Configuration<AuditConfiguration> Configuration { get; } = new("Anvil.Audit", new());
    internal static AuditConfiguration Instance => Configuration.Data;

    public string? MongoConnection { get; set; }
    public string MongoDatabaseName { get; set; } = "AnvilAudit";

    public string GetConnectionString()
    {
        if (string.IsNullOrEmpty(Instance.MongoConnection))
        {
            return Amethyst.Storages.StorageConfiguration.Instance.MongoConnection;
        }

        return Instance.MongoConnection;
    }
}