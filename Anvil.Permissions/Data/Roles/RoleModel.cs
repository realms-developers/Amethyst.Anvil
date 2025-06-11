using Amethyst.Network.Structures;
using Amethyst.Storages.Mongo;
using Anvil.Permissions.Data.Groups;
using Anvil.Permissions.Storage;
using MongoDB.Bson.Serialization.Attributes;

namespace Anvil.Permissions.Data.Roles;

[BsonIgnoreExtraElements]
public sealed class RoleModel : DataModel
{
    public RoleModel(string name) : base(name)
    {
        InternalGroup = new GroupModel(new($"role/{Name}"))
        {
            Tag = "role"
        };
    }

    public GroupModel InternalGroup { get; set; }
    public List<string> Groups { get; set; } = new();
    public bool IsDefault { get; set; }

    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public NetColor? Color { get; set; }

    public override void Save()
    {
        ModuleStorage.Roles.Save(this);
    }

    public override void Remove()
    {
        ModuleStorage.Roles.Remove(Name);
    }
}