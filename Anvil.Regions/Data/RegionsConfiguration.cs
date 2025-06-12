using Amethyst.Storages.Config;

namespace Anvil.ChatControl;

public sealed class RegionsConfiguration
{
    static RegionsConfiguration()
    {
        Configuration = new($"Anvil.Regions", new());
        Configuration.Load();
    }

    public static Configuration<RegionsConfiguration> Configuration { get; }
    public static RegionsConfiguration Instance => Configuration.Data;

    public bool SplitRegionsByProfiles { get; set; }
    public string? MongoConnection { get; set; }
    public string? MongoDatabaseName { get; set; }

    public string GetConnectionString()
    {
        if (string.IsNullOrEmpty(Instance.MongoConnection))
        {
            return Amethyst.Storages.StorageConfiguration.Instance.MongoConnection;
        }

        return Instance.MongoConnection;
    }

    public string GetStorageName()
    {
        if (string.IsNullOrEmpty(Instance.MongoDatabaseName))
        {
            return Amethyst.Storages.StorageConfiguration.Instance.MongoDatabaseName;
        }

        return Instance.MongoDatabaseName;
    }
}