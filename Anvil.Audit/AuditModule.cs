using Amethyst.Extensions.Base.Metadata;
using Amethyst.Extensions.Modules;
using Amethyst.Storages.Mongo;

namespace Anvil.Audit;

[ExtensionMetadata("Anvil.Audit", "realms-developers")]
public static class AuditModule
{
    internal static MongoDatabase Database = new MongoDatabase(
        AuditConfiguration.Instance.GetConnectionString(),
        AuditConfiguration.Instance.MongoDatabaseName
    );

    private static Dictionary<string, AuditInstance> _instances = new();
    public static IReadOnlyDictionary<string, AuditInstance> Instances => _instances.AsReadOnly();

    public static AuditInstance GetInstance(string name)
    {
        if (_instances.TryGetValue(name, out var instance))
        {
            return instance;
        }

        instance = new AuditInstance(name);
        _instances[name] = instance;
        return instance;
    }
}
