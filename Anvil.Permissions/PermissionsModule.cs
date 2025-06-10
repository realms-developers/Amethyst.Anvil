using Amethyst.Extensions.Base.Metadata;
using Amethyst.Extensions.Modules;
using Amethyst.Systems.Users;
using Anvil.Audit;
using Anvil.Permissions.Working;

namespace Anvil.Permissions;

[ExtensionMetadata("Anvil.Permissions", "realms-developers")]
public static class PermissionsModule
{
    public static AuditInstance AuditInstance { get; } = AuditModule.GetInstance("Anvil.Permissions");

    private static bool _initialized;

    [ModuleInitialize]
    public static void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        UsersOrganizer.PlayerUsers.PermissionProviderBuilder = new AnvilProviderBuilder();
    }
}
