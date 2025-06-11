using Amethyst.Permissions.Data.User;
using Amethyst.Systems.Users.Base.Permissions;
using Anvil.Permissions.Data.Roles;
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

    public List<string> PersonalPermissions { get; set; } = new();

    public RoleModel? RoleModel => TempRoleExpiration.HasValue && TempRoleExpiration.Value > DateTime.UtcNow
        ? TempRoleModel
        : RealRoleModel;

    public RoleModel? RealRoleModel { get; set; }
    public RoleModel? TempRoleModel { get; set; }

    public void Assign(UserModel model)
    {
        Unassign();

        if (model.InternalGroup != null && !model.InternalGroup.IsDisabled)
        {
            PersonalPermissions.AddRange(model.InternalGroup.Permissions);
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
            TempRoleExpiration = model.TempRole.Value.Item2;
            TempRoleModel = ModuleStorage.Roles.Find(model.TempRole.Value.Item1);

            List<string> handleRolePermissions = new();
            HandleRole(model.Role, ref handleRolePermissions);
            TempRolePermissions = handleRolePermissions;
        }

        if (!string.IsNullOrEmpty(model.Role))
        {
            RealRoleModel = ModuleStorage.Roles.Find(model.Role);

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
        PersonalPermissions = new List<string>();
        Permissions = new List<string>();
        TempRoleExpiration = null;
        TempRolePermissions = new List<string>();
        RealRolePermissions = new List<string>();
    }

    public PermissionAccess HasPermission(string permission)
    {
        if (permission.StartsWith("hasrole"))
        {
            return RoleModel != null && permission == $"hasrole<{RoleModel.Name}>"
                ? PermissionAccess.HasPermission
                : PermissionAccess.None;
        }
        
        if (permission.StartsWith("hasgroup"))
        {
            string groupName = permission.Substring(9, permission.Length - 10);
            return RoleModel != null && RoleModel.Groups.Contains(groupName)
                ? PermissionAccess.HasPermission
                : PermissionAccess.None;
        }

        if (permission.StartsWith("anvil.float"))
        {

        }

        if (string.IsNullOrEmpty(permission))
                return PermissionAccess.None;

        if (Permissions.Contains(permission))
            return PermissionAccess.HasPermission;

        if (Permissions.Contains("!" + permission))
            return PermissionAccess.Blocked;

        string[] permArray = permission.Split('.');
        return HasPartedPermission(permArray, Permissions) ?? HasPartedPermission(permArray, RolePermissions) ?? PermissionAccess.None;
    }

    private PermissionAccess? HasPartedPermission(string[] array, List<string> perms)
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

        return null;
    }
}