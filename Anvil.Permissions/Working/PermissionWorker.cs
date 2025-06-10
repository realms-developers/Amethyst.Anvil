using Amethyst.Permissions.Data.User;
using Amethyst.Systems.Users.Base.Permissions;
using Anvil.Permissions.Storage;

namespace Anvil.Permissions.Working;

public sealed class PermissionWorker
{
    public List<string> RolePermissions => TempRoleExpiration.HasValue && TempRoleExpiration.Value > DateTime.UtcNow
        ? TempRolePermissions
        : RealRolePermissions;

    public DateTime? TempRoleExpiration;
    public List<string> TempRolePermissions { get; set; } = new();
    public List<string> RealRolePermissions { get; set; } = new();

    public List<string> Permissions { get; set; } = new();

    public void Assign(UserModel model)
    {
        Unassign();

        if (model.InternalGroup != null && !model.InternalGroup.IsDisabled)
        {
            Permissions.AddRange(model.InternalGroup.Permissions);
        }
        if (model.Groups != null && model.Groups.Count > 0)
        {
            foreach (var group in model.Groups)
            {
                var groupModel = ModuleStorage.Groups.Find(group);
                if (groupModel != null && !groupModel.IsDisabled)
                {
                    Permissions.AddRange(groupModel.Permissions);
                }
            }
        }

        if (model.TempRole.HasValue && model.TempRole.Value.Item2 > DateTime.UtcNow)
        {
            List<string> handleRolePermissions = new();
            HandleRole(model.Role, ref handleRolePermissions);
            TempRolePermissions = handleRolePermissions;
        }

        if (!string.IsNullOrEmpty(model.Role))
        {
            List<string> handleRolePermissions = new();
            HandleRole(model.Role, ref handleRolePermissions);
            RealRolePermissions = handleRolePermissions;
        }
    }

    private void HandleRole(string? roleName, ref List<string> perms)
    {
        if (string.IsNullOrEmpty(roleName))
            return;

        var roleModel = ModuleStorage.Roles.Find(roleName);
        if (roleModel != null)
        {
            if (!roleModel.InternalGroup.IsDisabled)
                perms.AddRange(roleModel.InternalGroup.Permissions);

            foreach (var group in roleModel.Groups)
            {
                var groupModel = ModuleStorage.Groups.Find(group);
                if (groupModel != null && !groupModel.IsDisabled)
                {
                    perms.AddRange(groupModel.Permissions);
                }
            }
        }
    }

    public void Unassign()
    {
        Permissions = new List<string>();
        TempRoleExpiration = null;
        TempRolePermissions = new List<string>();
        RealRolePermissions = new List<string>();
    }

    private PermissionAccess HasPartedPermission(string[] array, List<string> perms)
    {
        foreach (string part in array)
        {
            if (perms.Contains("!" + part + ".*"))
            {
                return PermissionAccess.Blocked;
            }

            if (perms.Contains(part + ".*"))
            {
                return PermissionAccess.HasPermission;
            }
        }

        return PermissionAccess.None;
    }
}