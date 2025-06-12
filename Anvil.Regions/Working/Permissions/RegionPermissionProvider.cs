using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Base.Permissions;
using Anvil.Regions.Data.Models;

namespace Anvil.Regions.Working.Permissions;

public sealed class RegionPermissionProvider : IPermissionProvider
{
    public RegionPermissionProvider(IAmethystUser user)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
    }

    public IAmethystUser User { get; }

    public bool SupportsChildProviders => false;

    public void AddChild(IPermissionProvider provider)
    {
        throw new NotSupportedException("Child permission providers are not supported in PlayerRegionPermissionProvider.");
    }

    public bool HasChild<T>() where T : IPermissionProvider
    {
        throw new NotSupportedException("Child permission providers are not supported in PlayerRegionPermissionProvider.");
    }

    public void RemoveChild(IPermissionProvider provider)
    {
        throw new NotSupportedException("Child permission providers are not supported in PlayerRegionPermissionProvider.");
    }

    public void RemoveChild<T>() where T : IPermissionProvider
    {
        throw new NotSupportedException("Child permission providers are not supported in PlayerRegionPermissionProvider.");
    }

    public PermissionAccess HasPermission(string permission)
    {
        return PermissionAccess.None;
    }

    public PermissionAccess HasPermission(PermissionType type, int x, int y)
    {
        if (User.Suspensions?.IsSuspended == true)
        {
            return PermissionAccess.Blocked;
        }

        RegionModel? region = RegionsModule.Regions.FirstOrDefault(p => p.X <= x && p.X2 >= x && p.Y <= y && p.Y2 >= y);
        return HandleHasPermission(region, type);
    }

    public PermissionAccess HasPermission(PermissionType type, int x, int y, int width, int height)
    {
        if (User.Suspensions?.IsSuspended == true)
        {
            return PermissionAccess.Blocked;
        }

        RegionModel? region = RegionsModule.Regions.FirstOrDefault(r =>
            r.X <= x + width - 1 &&
            r.X2 >= x &&
            r.Y <= y + height - 1 &&
            r.Y2 >= y
        );
        return HandleHasPermission(region, type);
    }

    private PermissionAccess HandleHasPermission(RegionModel? region, PermissionType type)
    {
        if (region == null)
            return PermissionAccess.None;

        if (!CheckRegionSettings(region, type))
        {
            User.Messages.ReplyError("regions.youCantBuildHere");
            return PermissionAccess.Blocked;
        }

        return PermissionAccess.HasPermission;
    }
    
    private bool CheckRegionSettings(RegionModel region, PermissionType type)
    {
        string typeName = type.ToString().ToLowerInvariant();

        if (region.Tags.Contains("block-all:" + typeName))
            return false;

        if (region.Tags.Contains("allow-all:" + typeName))
            return true;

        if (region.Members.Any(p => p.Name == User.Name))
            return true;

        if (region.Roles.Any(p => User.Permissions.HasPermission($"hasrole<{p.Name}>") == PermissionAccess.HasPermission))
            return true;

        return true;
    }
}
