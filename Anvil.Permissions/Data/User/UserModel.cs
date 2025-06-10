using Amethyst.Storages.Mongo;
using Anvil.Permissions.Data.Groups;
using Anvil.Permissions.Storage;
using MongoDB.Bson.Serialization.Attributes;

namespace Amethyst.Permissions.Data.User;

[BsonIgnoreExtraElements]
public sealed class UserModel : DataModel
{
    public UserModel(string name) : base(name)
    {
        InternalGroup = new GroupModel(new($"user/{Name}"))
        {
            Tag = "user"
        };
    }

    public GroupModel InternalGroup { get; set; }
    public List<string> Groups { get; set; } = new();
    public string? Role { get; set; } = string.Empty;

    public (string, DateTime)? TempRole { get; set; }

    public override void Save()
    {
        ModuleStorage.Users.Save(this);
    }

    public override void Remove()
    {
        ModuleStorage.Users.Remove(Name);
    }
}