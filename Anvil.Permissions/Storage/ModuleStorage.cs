using Amethyst.Permissions.Data.User;
using Amethyst.Permissions.Storage;
using Amethyst.Storages.Mongo;
using Anvil.Permissions.Data.Groups;
using Anvil.Permissions.Data.Roles;

namespace Anvil.Permissions.Storage;

public static class ModuleStorage
{
    public static MongoDatabase GroupDatabase { get; }
        = new(ConfigManager.GroupStorage.GetConnectionString(), ConfigManager.GroupStorage.GetStorageName());
    public static MongoModels<GroupModel> Groups { get; } = GroupDatabase.Get<GroupModel>("Groups");

    public static MongoDatabase RoleDatabase { get; }
        = new(ConfigManager.RoleStorage.GetConnectionString(), ConfigManager.RoleStorage.GetStorageName());
    public static MongoModels<RoleModel> Roles { get; } = RoleDatabase.Get<RoleModel>("Roles");

    public static MongoDatabase UserDatabase { get; }
        = new(ConfigManager.UserStorage.GetConnectionString(), ConfigManager.UserStorage.GetStorageName());
    public static MongoModels<UserModel> Users { get; } = UserDatabase.Get<UserModel>("Users");
}