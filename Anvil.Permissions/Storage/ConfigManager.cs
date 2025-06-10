using Amethyst.Permissions.Data.User;
using Anvil.Permissions.Data.Groups;
using Anvil.Permissions.Data.Roles;
using Anvil.Permissions.Storage;

namespace Amethyst.Permissions.Storage;

public static class ConfigManager
{
    public static StorageConfiguration<GroupModel> GroupStorage { get; } = new();
    public static StorageConfiguration<RoleModel> RoleStorage { get; } = new();
    public static StorageConfiguration<UserModel> UserStorage { get; } = new();
}