using Amethyst.Storages.Mongo;
using Anvil.Regions.Data.Models;

namespace Anvil.Regions.Data;

public static class ModuleStorage
{
    public static MongoDatabase RegionDatabase { get; }
        = new(RegionsConfiguration.Instance.GetConnectionString(), RegionsConfiguration.Instance.GetStorageName());
    public static MongoModels<RegionModel> Regions { get; } = RegionDatabase.Get<RegionModel>("Regions");
}