using Amethyst.Network.Structures;
using Amethyst.Storages.Mongo;
using Anvil.Essentials.Data;
using MongoDB.Bson.Serialization.Attributes;

namespace Amethyst.Essentials.Data.Models;

[BsonIgnoreExtraElements]
public sealed class WarpModel : DataModel
{
    public WarpModel(string name) : base(name)
    {
    }

    public NetVector2 Position { get; set; }

    public override void Save()
    {
        PluginStorage.Regions.Save(this);
    }

    public override void Remove()
    {
        PluginStorage.Regions.Remove(Name);
    }
}
