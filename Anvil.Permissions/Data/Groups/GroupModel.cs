using Amethyst.Storages.Mongo;
using Anvil.Permissions.Storage;
using MongoDB.Bson.Serialization.Attributes;

namespace Anvil.Permissions.Data.Groups;

[BsonIgnoreExtraElements]
public sealed class GroupModel : DataModel
{
    public GroupModel(string name) : base(name)
    {
    }

    public string? Tag { get; set; }
    public bool IsDisabled { get; set; }
    public List<string> Permissions { get; set; } = new();

    public override void Save()
    {
        ModuleStorage.Groups.Save(this);
    }

    public override void Remove()
    {
        ModuleStorage.Groups.Remove(Name);
    }
}