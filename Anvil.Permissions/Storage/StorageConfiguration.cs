using System.Text.Json.Serialization;
using Amethyst.Storages.Config;
using Amethyst.Storages.Mongo;

namespace Anvil.Permissions.Storage;

public sealed class StorageConfiguration<T> where T : DataModel
{
    public StorageConfiguration() => Configuration.Load();

    [JsonIgnore]
    public Configuration<StorageConfiguration<T>> Configuration { get; } = new($"Anvil.{nameof(T)}", new());
    [JsonIgnore]
    public StorageConfiguration<T> Instance => Configuration.Data;

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