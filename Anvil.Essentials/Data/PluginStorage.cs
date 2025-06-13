using Amethyst.Essentials.Data.Models;
using Amethyst.Storages.Mongo;

namespace Anvil.Essentials.Data;

public static class PluginStorage
{
    public static MongoDatabase RegionDatabase { get; }
        = new(EssentialsConfiguration.Instance.GetConnectionString(), EssentialsConfiguration.Instance.GetStorageName());
    public static MongoModels<WarpModel> Regions { get; } = RegionDatabase.Get<WarpModel>("Warps");
}