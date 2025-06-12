using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Base.Permissions;

namespace Anvil.Regions.Working.Users;

public sealed class ExecutorPermissionProvider : IPermissionProvider
{
    public ExecutorPermissionProvider(IAmethystUser user)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
    }

    public IAmethystUser User { get; }

    public bool SupportsChildProviders => false;

    public void AddChild(IPermissionProvider provider)
    {
        throw new NotSupportedException("Child permission providers are not supported in RegionsPermissionProvider.");
    }
    
    public void RemoveChild(IPermissionProvider provider)
    {
        throw new NotSupportedException("Child permission providers are not supported in RegionsPermissionProvider.");
    }

    public void RemoveChild<T>() where T : IPermissionProvider
    {
        throw new NotSupportedException("Child permission providers are not supported in RegionsPermissionProvider.");
    }

    public bool HasChild<T>() where T : IPermissionProvider
    {
        throw new NotSupportedException("Child permission providers are not supported in RegionsPermissionProvider.");
    }

    public PermissionAccess HasPermission(string permission)
    {
        if (permission.StartsWith("anvil"))
            return PermissionAccess.Blocked;

        return PermissionAccess.HasPermission;
    }

    public PermissionAccess HasPermission(PermissionType type, int x, int y)
    {
        return PermissionAccess.HasPermission;
    }

    public PermissionAccess HasPermission(PermissionType type, int x, int y, int width, int height)
    {
        return PermissionAccess.HasPermission;
    }
}