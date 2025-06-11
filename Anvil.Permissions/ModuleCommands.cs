using Amethyst.Network.Structures;
using Amethyst.Server.Entities;
using Amethyst.Systems.Commands.Base;
using Amethyst.Systems.Commands.Dynamic.Attributes;
using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Base.Permissions;
using Amethyst.Text;
using Anvil.Permissions.Data.Groups;
using Anvil.Permissions.Data.Roles;
using Anvil.Permissions.Storage;
using Anvil.Permissions.Working;

namespace Anvil.Permissions;

public static class ModuleCommands
{
    public static bool IsLocked { get; set; } = true;

    [Command(["ar lock"], "amethyst.desc.permissions.lock")]
    [CommandRepository("root")]
    [CommandPermission("anvil.permissions.lock")]
    public static void AnvilRootLock(IAmethystUser user, CommandInvokeContext ctx)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.alreadyLocked");
            return;
        }

        IsLocked = true;
        ctx.Messages.ReplySuccess("amethyst.reply.permissions.locked");

        PermissionsModule.AuditInstance.Log("anvilroot.lock", "Anvil.Permissions was locked", ["anvilroot", "anvilperms", "warning"], user.Name);
    }

    [Command(["ar unlock"], "amethyst.desc.permissions.unlock")]
    [CommandRepository("root")]
    [CommandPermission("anvil.permissions.unlock")]
    public static void AnvilRootUnlock(IAmethystUser user, CommandInvokeContext ctx)
    {
        if (!IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.alreadyUnlocked");
            return;
        }

        IsLocked = false;
        ctx.Messages.ReplySuccess("amethyst.reply.permissions.unlocked");
        PermissionsModule.AuditInstance.Log("anvilroot.unlock", "Anvil.Permissions was unlocked", ["anvilroot", "anvilperms", "info"], user.Name);
    }

    [Command(["apply-perms"], "amethyst.desc.permissions.applypermissions")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.applypermissions")]
    public static void ApplyPermissions(IAmethystUser user, CommandInvokeContext ctx)
    {
        foreach (var plr in EntityTrackers.Players)
        {
            if (plr.User != null && plr.User.Permissions is AnvilPermissionProvider provider)
            {
                var permUser = ModuleStorage.Users.Find(plr.User.Name);

                if (permUser != null)
                    provider.Worker.Assign(permUser);
            }
        }

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.changesWasApplied");
    }

    [Command(["mrole create"], "amethyst.desc.permissions.mrole.create")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mrole.create")]
    [CommandSyntax("en-US", "<name>")]
    [CommandSyntax("ru-RU", "<имя>")]
    public static void RoleCreate(IAmethystUser user, CommandInvokeContext ctx, string name)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNameRequired");
            return;
        }

        if (ModuleStorage.Roles.Find(name) != null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleAlreadyExists", name);
            return;
        }

        var role = new RoleModel(name);
        role.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.roleCreated", name);
        PermissionsModule.AuditInstance.Log("mrole.create", $"Role '{name}' was created", [$"anvilperms.role:{name}", "anvilperms", "info"], user.Name, new()
        {
            {"role", name}
        });
    }

    [Command(["mrole remove"], "amethyst.desc.permissions.mrole.remove")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mrole.remove")]
    [CommandSyntax("en-US", "<name>")]
    [CommandSyntax("ru-RU", "<имя>")]
    public static void RoleRemove(IAmethystUser user, CommandInvokeContext ctx, string name)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNameRequired");
            return;
        }

        var role = ModuleStorage.Roles.Find(name);
        if (role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotFound", name);
            return;
        }

        role.Remove();
        ctx.Messages.ReplySuccess("amethyst.reply.permissions.roleRemoved", name);
        PermissionsModule.AuditInstance.Log("mrole.remove", $"Role '{name}' was removed", [$"anvilperms.role:{name}", "anvilperms", "info"], user.Name, new()
        {
            {"role", name}
        });
    }

    [Command(["mrole list"], "amethyst.desc.permissions.mrole.list")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mrole.list")]
    [CommandSyntax("en-US", "[page]")]
    [CommandSyntax("ru-RU", "[страница]")]
    public static void RoleList(IAmethystUser user, CommandInvokeContext ctx, int page = 0)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        var roles = ModuleStorage.Roles.FindAll();
        if (roles.Count() == 0)
        {
            ctx.Messages.ReplyInfo("amethyst.reply.permissions.noRolesFound");
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(roles.Select(p => p.Name));
        ctx.Messages.ReplyPage(pages, "amethyst.reply.permissions.rolesList", null, null, false, page);
    }

    [Command(["mrole prefix"], "amethyst.desc.permissions.mrole.prefix")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mrole.prefix")]
    [CommandSyntax("en-US", "<role>", "[prefix]")]
    [CommandSyntax("ru-RU", "<роль>", "[префикс]")]
    public static void RolePrefix(IAmethystUser user, CommandInvokeContext ctx, string roleName, string? prefix = null)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(roleName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNameRequired");
            return;
        }

        var role = ModuleStorage.Roles.Find(roleName);
        if (role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotFound", roleName);
            return;
        }

        role.Prefix = prefix;
        role.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.rolePrefixSet", role.Name, prefix ?? "null");
        PermissionsModule.AuditInstance.Log("mrole.prefix", $"Role '{role.Name}' prefix was set to '{prefix ?? "null"}'",
            [$"anvilperms.role:{role.Name}", "anvilperms", "info"], user.Name, new()
        {
            {"role", role.Name},
            {"prefix", prefix ?? "null"}
        });
    }

    [Command(["mrole suffix"], "amethyst.desc.permissions.mrole.suffix")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mrole.suffix")]
    [CommandSyntax("en-US", "<role>", "[suffix]")]
    [CommandSyntax("ru-RU", "<роль>", "[суффикс]")]
    public static void RoleSuffix(IAmethystUser user, CommandInvokeContext ctx, string roleName, string? suffix = null)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(roleName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNameRequired");
            return;
        }

        var role = ModuleStorage.Roles.Find(roleName);
        if (role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotFound", roleName);
            return;
        }

        role.Suffix = suffix;
        role.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.roleSuffixSet", role.Name, suffix ?? "null");
        PermissionsModule.AuditInstance.Log("mrole.suffix", $"Role '{role.Name}' suffix was set to '{suffix ?? "null"}'",
            [$"anvilperms.role:{role.Name}", "anvilperms", "info"], user.Name, new()
        {
            {"role", role.Name},
            {"suffix", suffix ?? "null"}
        });
    }

    [Command(["mrole color"], "amethyst.desc.permissions.mrole.color")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mrole.color")]
    [CommandSyntax("en-US", "<role>", "[#HHEEXX | RRR,GGG,BBB]")]
    [CommandSyntax("ru-RU", "<роль>", "[#HHEEXX | RRR,GGG,BBB]")]
    public static void RoleColor(IAmethystUser user, CommandInvokeContext ctx, string roleName, NetColor? color = null)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(roleName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNameRequired");
            return;
        }

        var role = ModuleStorage.Roles.Find(roleName);
        if (role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotFound", roleName);
            return;
        }

        role.Color = color;
        role.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.roleColorSet", role.Name, color?.ToHex() ?? "null");
        PermissionsModule.AuditInstance.Log("mrole.color", $"Role '{role.Name}' color was set to '{color?.ToHex() ?? "null"}'",
            [$"anvilperms.role:{role.Name}", "anvilperms", "info"], user.Name, new()
        {
            {"role", role.Name},
            {"color", color?.ToHex() ?? "null"}
        });
    }

    [Command(["mrole addgroup"], "amethyst.desc.permissions.mrole.addgroup")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mrole.addgroup")]
    [CommandSyntax("en-US", "<role>", "<group>")]
    [CommandSyntax("ru-RU", "<роль>", "<группа>")]
    public static void RoleAddGroup(IAmethystUser user, CommandInvokeContext ctx, string roleName, string groupName)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(roleName) || string.IsNullOrWhiteSpace(groupName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleAndGroupRequired");
            return;
        }

        var role = ModuleStorage.Roles.Find(roleName);
        if (role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotFound", roleName);
            return;
        }

        var group = ModuleStorage.Groups.Find(groupName);
        if (group == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupNotFound", groupName);
            return;
        }

        if (role.Groups.Contains(group.Name))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupAlreadyInRole", group.Name, role.Name);
            return;
        }

        role.Groups.Add(group.Name);
        role.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.groupAddedToRole", group.Name, role.Name);
        PermissionsModule.AuditInstance.Log("mrole.addgroup", $"Group '{group.Name}' was added to role '{role.Name}'",
            [$"anvilperms.role:{role.Name}", $"anvilperms.group:{group.Name}", "anvilperms", "info"], user.Name, new()
        {
            {"role", role.Name},
            {"group", group.Name}
        });
    }

    [Command(["mrole removegroup"], "amethyst.desc.permissions.mrole.removegroup")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mrole.removegroup")]
    [CommandSyntax("en-US", "<role>", "<group>")]
    [CommandSyntax("ru-RU", "<роль>", "<группа>")]
    public static void RoleRemoveGroup(IAmethystUser user, CommandInvokeContext ctx, string roleName, string groupName)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(roleName) || string.IsNullOrWhiteSpace(groupName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleAndGroupRequired");
            return;
        }

        var role = ModuleStorage.Roles.Find(roleName);
        if (role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotFound", roleName);
            return;
        }

        if (!role.Groups.Contains(groupName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupNotInRole", groupName, role.Name);
            return;
        }

        role.Groups.Remove(groupName);
        role.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.groupRemovedFromRole", groupName, role.Name);
        PermissionsModule.AuditInstance.Log("mrole.removegroup", $"Group '{groupName}' was removed from role '{role.Name}'",
            [$"anvilperms.role:{role.Name}", $"anvilperms.group:{groupName}", "anvilperms", "info"], user.Name, new()
        {
            {"role", role.Name},
            {"group", groupName}
        });
    }

    [Command(["mrole setdefault"], "amethyst.desc.permissions.mrole.setdefault")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mrole.setdefault")]
    [CommandSyntax("en-US", "<role>")]
    [CommandSyntax("ru-RU", "<роль>")]
    public static void RoleSetDefault(IAmethystUser user, CommandInvokeContext ctx, string roleName)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(roleName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNameRequired");
            return;
        }

        var role = ModuleStorage.Roles.Find(roleName);
        if (role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotFound", roleName);
            return;
        }

        foreach (var r in ModuleStorage.Roles.FindAll())
        {
            r.IsDefault = false;
            r.Save();
        }

        role.IsDefault = true;
        role.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.roleSetAsDefault", role.Name);
        PermissionsModule.AuditInstance.Log("mrole.setdefault", $"Role '{role.Name}' was set as default",
            [$"anvilperms.role:{role.Name}", "anvilperms", "info"], user.Name, new()
        {
            {"role", role.Name}
        });
    }

    [Command(["mrole unsetdefault"], "amethyst.desc.permissions.mrole.unsetdefault")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mrole.unsetdefault")]
    [CommandSyntax("en-US", "<role>")]
    [CommandSyntax("ru-RU", "<роль>")]
    public static void RoleUnsetDefault(IAmethystUser user, CommandInvokeContext ctx, string roleName)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(roleName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNameRequired");
            return;
        }

        var role = ModuleStorage.Roles.Find(roleName);
        if (role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotFound", roleName);
            return;
        }

        if (!role.IsDefault)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotDefault", role.Name);
            return;
        }

        role.IsDefault = false;
        role.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.roleUnsetAsDefault", role.Name);
        PermissionsModule.AuditInstance.Log("mrole.unsetdefault", $"Role '{role.Name}' was unset as default",
            [$"anvilperms.role:{role.Name}", "anvilperms", "info"], user.Name, new()
        {
            {"role", role.Name}
        });
    }

    [Command(["mrole addperm"], "amethyst.desc.permissions.mrole.addperm")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mrole.addperm")]
    [CommandSyntax("en-US", "<role>", "<permission>")]
    [CommandSyntax("ru-RU", "<роль>", "<разрешение>")]
    public static void RoleAddPermission(IAmethystUser user, CommandInvokeContext ctx, string roleName, string permission)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(roleName) || string.IsNullOrWhiteSpace(permission))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleAndPermissionRequired");
            return;
        }

        var role = ModuleStorage.Roles.Find(roleName);
        if (role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotFound", roleName);
            return;
        }

        if (role.InternalGroup.Permissions.Contains(permission))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.permissionAlreadyInRole", permission, role.Name);
            return;
        }

        role.InternalGroup.Permissions.Add(permission);
        role.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.permissionAddedToRole", permission, role.Name);
        PermissionsModule.AuditInstance.Log("mrole.addperm", $"Permission '{permission}' was added to role '{role.Name}'",
            [$"anvilperms.role:{role.Name}", $"anvilperms.permission:{permission}", "anvilperms", "info"], user.Name, new()
        {
            {"role", role.Name},
            {"permission", permission}
        });
    }

    [Command(["mrole removeperm"], "amethyst.desc.permissions.mrole.removeperm")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mrole.removeperm")]
    [CommandSyntax("en-US", "<role>", "<permission>")]
    [CommandSyntax("ru-RU", "<роль>", "<разрешение>")]
    public static void RoleRemovePermission(IAmethystUser user, CommandInvokeContext ctx, string roleName, string permission)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(roleName) || string.IsNullOrWhiteSpace(permission))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleAndPermissionRequired");
            return;
        }

        var role = ModuleStorage.Roles.Find(roleName);
        if (role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotFound", roleName);
            return;
        }

        if (!role.InternalGroup.Permissions.Contains(permission))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.permissionNotInRole", permission, role.Name);
            return;
        }

        role.InternalGroup.Permissions.Remove(permission);
        role.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.permissionRemovedFromRole", permission, role.Name);
        PermissionsModule.AuditInstance.Log("mrole.removeperm", $"Permission '{permission}' was removed from role '{role.Name}'",
            [$"anvilperms.role:{role.Name}", $"anvilperms.permission:{permission}", "anvilperms", "info"], user.Name, new()
        {
            {"role", role.Name},
            {"permission", permission}
        });
    }

    [Command(["mrole listperm"], "amethyst.desc.permissions.mrole.listperm")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mrole.info")]
    [CommandSyntax("en-US", "<role>", "[page]")]
    [CommandSyntax("ru-RU", "<роль>", "[страница]")]
    public static void RoleListPermissions(IAmethystUser user, CommandInvokeContext ctx, string roleName, int page = 0)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNameRequired");
            return;
        }

        var role = ModuleStorage.Roles.Find(roleName);
        if (role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotFound", roleName);
            return;
        }

        var permissions = role.InternalGroup.Permissions;
        if (permissions.Count == 0)
        {
            ctx.Messages.ReplyInfo("amethyst.reply.permissions.noPermissionsInRole", role.Name);
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(permissions);
        ctx.Messages.ReplyPage(pages, "amethyst.reply.permissions.rolePermissionsList", null, null, false, page);
    }

    [Command(["mrole listgroups"], "amethyst.desc.permissions.mrole.listgroups")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mrole.info")]
    [CommandSyntax("en-US", "<role>", "[page]")]
    [CommandSyntax("ru-RU", "<роль>", "[страница]")]
    public static void RoleListGroups(IAmethystUser user, CommandInvokeContext ctx, string roleName, int page = 0)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNameRequired");
            return;
        }

        var role = ModuleStorage.Roles.Find(roleName);
        if (role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotFound", roleName);
            return;
        }

        if (role.Groups.Count == 0)
        {
            ctx.Messages.ReplyInfo("amethyst.reply.permissions.noGroupsInRole", role.Name);
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(role.Groups);
        ctx.Messages.ReplyPage(pages, "amethyst.reply.permissions.roleGroupsList", null, null, false, page);
    }

    [Command(["mrole listusers"], "amethyst.desc.permissions.mrole.listusers")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mrole.info")]
    [CommandSyntax("en-US", "<role>", "[page]")]
    [CommandSyntax("ru-RU", "<роль>", "[страница]")]
    public static void RoleListUsers(IAmethystUser user, CommandInvokeContext ctx, string roleName, int page = 0)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNameRequired");
            return;
        }

        var role = ModuleStorage.Roles.Find(roleName);
        if (role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotFound", roleName);
            return;
        }

        var users = ModuleStorage.Users.FindAll(p => p.Role == roleName);
        if (users.Count() == 0)
        {
            ctx.Messages.ReplyInfo("amethyst.reply.permissions.noUsersInRole", role.Name);
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(users.Select(u => u.Name));
        ctx.Messages.ReplyPage(pages, "amethyst.reply.permissions.roleUsersList", null, null, false, page);
    }


    [Command(["mgroup create"], "amethyst.desc.permissions.mgroup.create")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mgroup.create")]
    [CommandSyntax("en-US", "<name>")]
    [CommandSyntax("ru-RU", "<имя>")]
    public static void GroupCreate(IAmethystUser user, CommandInvokeContext ctx, string name)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupNameRequired");
            return;
        }

        if (ModuleStorage.Groups.Find(name) != null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupAlreadyExists", name);
            return;
        }

        var group = new GroupModel(name);
        group.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.groupCreated", name);
        PermissionsModule.AuditInstance.Log("mgroup.create", $"Group '{name}' was created", [$"anvilperms.group:{name}", "anvilperms", "info"], user.Name, new()
        {
            {"group", name}
        });
    }

    [Command(["mgroup remove"], "amethyst.desc.permissions.mgroup.remove")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mgroup.remove")]
    [CommandSyntax("en-US", "<name>")]
    [CommandSyntax("ru-RU", "<имя>")]
    public static void GroupRemove(IAmethystUser user, CommandInvokeContext ctx, string name)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupNameRequired");
            return;
        }

        var group = ModuleStorage.Groups.Find(name);
        if (group == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupNotFound", name);
            return;
        }

        group.Remove();
        ctx.Messages.ReplySuccess("amethyst.reply.permissions.groupRemoved", name);
        PermissionsModule.AuditInstance.Log("mgroup.remove", $"Group '{name}' was removed", [$"anvilperms.group:{name}", "anvilperms", "info"], user.Name, new()
        {
            {"group", name}
        });
    }

    [Command(["mgroup list"], "amethyst.desc.permissions.mgroup.list")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mgroup.list")]
    [CommandSyntax("en-US", "[page]")]
    [CommandSyntax("ru-RU", "[страница]")]
    public static void GroupList(IAmethystUser user, CommandInvokeContext ctx, int page = 0)
    {
        var groups = ModuleStorage.Groups.FindAll();
        if (groups.Count() == 0)
        {
            ctx.Messages.ReplyInfo("amethyst.reply.permissions.noGroupsFound");
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(groups.Select(p => p.Name));
        ctx.Messages.ReplyPage(pages, "amethyst.reply.permissions.groupsList", null, null, false, page);
    }

    [Command(["mgroup addperm"], "amethyst.desc.permissions.mgroup.addperm")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mgroup.addperm")]
    [CommandSyntax("en-US", "<group>", "<permission>")]
    [CommandSyntax("ru-RU", "<группа>", "<разрешение>")]
    public static void GroupAddPermission(IAmethystUser user, CommandInvokeContext ctx, string groupName, string permission)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(groupName) || string.IsNullOrWhiteSpace(permission))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupAndPermissionRequired");
            return;
        }

        var group = ModuleStorage.Groups.Find(groupName);
        if (group == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupNotFound", groupName);
            return;
        }

        if (group.Permissions.Contains(permission))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.permissionAlreadyInGroup", permission, group.Name);
            return;
        }

        group.Permissions.Add(permission);
        group.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.permissionAddedToGroup", permission, group.Name);
        PermissionsModule.AuditInstance.Log("mgroup.addperm", $"Permission '{permission}' was added to group '{group.Name}'",
            [$"anvilperms.group:{group.Name}", $"anvilperms.permission:{permission}", "anvilperms", "info"], user.Name, new()
        {
            {"group", group.Name},
            {"permission", permission}
        });
    }

    [Command(["mgroup removeperm"], "amethyst.desc.permissions.mgroup.removeperm")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mgroup.removeperm")]
    [CommandSyntax("en-US", "<group>", "<permission>")]
    [CommandSyntax("ru-RU", "<группа>", "<разрешение>")]
    public static void GroupRemovePermission(IAmethystUser user, CommandInvokeContext ctx, string groupName, string permission)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(groupName) || string.IsNullOrWhiteSpace(permission))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupAndPermissionRequired");
            return;
        }

        var group = ModuleStorage.Groups.Find(groupName);
        if (group == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupNotFound", groupName);
            return;
        }

        if (!group.Permissions.Contains(permission))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.permissionNotInGroup", permission, group.Name);
            return;
        }

        group.Permissions.Remove(permission);
        group.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.permissionRemovedFromGroup", permission, group.Name);
        PermissionsModule.AuditInstance.Log("mgroup.removeperm", $"Permission '{permission}' was removed from group '{group.Name}'",
            [$"anvilperms.group:{group.Name}", $"anvilperms.permission:{permission}", "anvilperms", "info"], user.Name, new()
        {
            {"group", group.Name},
            {"permission", permission}
        });
    }

    [Command(["mgroup listperm"], "amethyst.desc.permissions.mgroup.listperm")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mgroup.info")]
    [CommandSyntax("en-US", "<group>", "[page]")]
    [CommandSyntax("ru-RU", "<группа>", "[страница]")]
    public static void GroupListPermissions(IAmethystUser user, CommandInvokeContext ctx, string groupName, int page = 0)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupNameRequired");
            return;
        }

        var group = ModuleStorage.Groups.Find(groupName);
        if (group == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupNotFound", groupName);
            return;
        }

        var permissions = group.Permissions;
        if (permissions.Count == 0)
        {
            ctx.Messages.ReplyInfo("amethyst.reply.permissions.noPermissionsInGroup", group.Name);
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(permissions);
        ctx.Messages.ReplyPage(pages, "amethyst.reply.permissions.groupPermissionsList", null, null, false, page);
    }

    [Command(["mgroup listpusers"], "amethyst.desc.permissions.mgroup.listpersonalusers")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.mgroup.info")]
    [CommandSyntax("en-US", "<group>", "[page]")]
    [CommandSyntax("ru-RU", "<группа>", "[страница]")]
    public static void GroupListPersonalUsers(IAmethystUser user, CommandInvokeContext ctx, string groupName, int page = 0)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupNameRequired");
            return;
        }

        var group = ModuleStorage.Groups.Find(groupName);
        if (group == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupNotFound", groupName);
            return;
        }

        var users = ModuleStorage.Users.FindAll(p => p.Groups.Contains(group.Name));
        if (users.Count() == 0)
        {
            ctx.Messages.ReplyInfo("amethyst.reply.permissions.noUsersInGroup", group.Name);
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(users.Select(u => u.Name));
        ctx.Messages.ReplyPage(pages, "amethyst.reply.permissions.groupUsersList", null, null, false, page);
    }

    [Command(["musr assign"], "amethyst.desc.permissions.musr.assign")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.musr.assign")]
    [CommandSyntax("en-US", "<user>", "<role>")]
    [CommandSyntax("ru-RU", "<пользователь>", "<роль>")]
    public static void UserAssignRole(IAmethystUser user, CommandInvokeContext ctx, string userName, string roleName)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(roleName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userAndRoleRequired");
            return;
        }

        var targetUser = ModuleStorage.Users.Find(userName);
        if (targetUser == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userNotFound", userName);
            return;
        }

        var role = ModuleStorage.Roles.Find(roleName);
        if (role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotFound", roleName);
            return;
        }

        if (user.Permissions.HasPermission($"anvil.float.assign<{roleName}>") != PermissionAccess.HasPermission && !user.Commands.Repositories.Contains("root"))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cantAssignRole");
            ctx.Messages.ReplyError("amethyst.reply.permissions.requiredPermissionIs", $"anvil.personal.assign<{roleName}>");
            return;
        }

        targetUser.Role = role.Name;
        targetUser.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.roleAssignedToUser", role.Name, targetUser.Name);
        PermissionsModule.AuditInstance.Log("musr.assign", $"Role '{role.Name}' was assigned to user '{targetUser.Name}'",
            [$"anvilperms.targetuser:{targetUser.Name}", $"anvilperms.role:{role.Name}", "anvilperms", "info"], user.Name, new()
        {
            {"user", targetUser.Name},
            {"role", role.Name}
        });
    }

    [Command(["musr unassign"], "amethyst.desc.permissions.musr.unassign")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.musr.unassign")]
    [CommandSyntax("en-US", "<user>")]
    [CommandSyntax("ru-RU", "<пользователь>")]
    public static void UserUnassignRole(IAmethystUser user, CommandInvokeContext ctx, string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userAndRoleRequired");
            return;
        }

        var targetUser = ModuleStorage.Users.Find(userName);
        if (targetUser == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userNotFound", userName);
            return;
        }

        if (targetUser.Role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userHasNoRole", targetUser.Name);
            return;
        }

        if (user.Permissions.HasPermission($"anvil.float.unassign<{targetUser.Role}>") != PermissionAccess.HasPermission && !user.Commands.Repositories.Contains("root"))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cantUnassignRole");
            ctx.Messages.ReplyError("amethyst.reply.permissions.requiredPermissionIs", $"anvil.personal.unassign<{targetUser.Role}>");
            return;
        }

        string roleName = targetUser.Role;
        targetUser.Role = null;
        targetUser.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.roleUnassignedFromUser", roleName, targetUser.Name);
        PermissionsModule.AuditInstance.Log("musr.unassign", $"Role '{roleName}' was unassigned from user '{targetUser.Name}'",
            [$"anvilperms.targetuser:{targetUser.Name}", $"anvilperms.role:{roleName}", "anvilperms", "info"], user.Name, new()
        {
            {"user", targetUser.Name},
            {"role", roleName}
        });
    }

    [Command(["musr tempassign"], "amethyst.desc.permissions.musr.tempassign")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.musr.tempassign")]
    [CommandSyntax("en-US", "<user>", "<role>", "[duration (X g/M/d/h/m/s)]")]
    [CommandSyntax("ru-RU", "<пользователь>", "<роль>", "[продолжительность (X г/М/д/ч/м/с)]")]
    public static void UserTempAssignRole(IAmethystUser user, CommandInvokeContext ctx, string userName, string roleName, string? time = null)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(roleName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userAndRoleRequired");
            return;
        }

        var targetUser = ModuleStorage.Users.Find(userName);
        if (targetUser == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userNotFound", userName);
            return;
        }

        var role = ModuleStorage.Roles.Find(roleName);
        if (role == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.roleNotFound", roleName);
            return;
        }

        if (user.Permissions.HasPermission($"anvil.float.tempassign<{roleName}>") != PermissionAccess.HasPermission && !user.Commands.Repositories.Contains("root"))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cantTempAssignRole");
            ctx.Messages.ReplyError("amethyst.reply.permissions.requiredPermissionIs", $"anvil.personal.tempassign<{roleName}>");
            return;
        }

        TimeSpan span = time == null ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(TextUtility.ParseToSeconds(time));

        targetUser.TempRole = (role.Name, DateTime.UtcNow.Add(span));
        targetUser.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.roleTempAssignedToUser", role.Name, targetUser.Name);
        PermissionsModule.AuditInstance.Log("musr.tempassign", $"Role '{role.Name}' was temporarily assigned to user '{targetUser.Name}'",
            [$"anvilperms.targetuser:{targetUser.Name}", $"anvilperms.role:{role.Name}", "anvilperms", "info"], user.Name, new()
        {
            {"user", targetUser.Name},
            {"role", role.Name},
            {"duration", span.ToString()},
            {"expiration", DateTime.UtcNow.Add(span).ToString()}
        });
    }

    [Command(["musr tempunassign"], "amethyst.desc.permissions.musr.tempunassign")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.musr.tempunassign")]
    [CommandSyntax("en-US", "<user>")]
    [CommandSyntax("ru-RU", "<пользователь>")]
    public static void UserTempUnassignRole(IAmethystUser user, CommandInvokeContext ctx, string userName)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userRequired");
            return;
        }

        var targetUser = ModuleStorage.Users.Find(userName);
        if (targetUser == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userNotFound", userName);
            return;
        }

        if (targetUser.TempRole == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userHasNoTempRole", targetUser.Name);
            return;
        }

        if (user.Permissions.HasPermission($"anvil.float.tempunassign<{targetUser.TempRole.Value.Item1}>") != PermissionAccess.HasPermission && !user.Commands.Repositories.Contains("root"))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cantTempUnassignRole");
            ctx.Messages.ReplyError("amethyst.reply.permissions.requiredPermissionIs", $"anvil.personal.tempunassign<{targetUser.TempRole.Value.Item1}>");
            return;
        }

        string roleName = targetUser.TempRole.Value.Item1;
        targetUser.TempRole = null;
        targetUser.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.roleTempUnassignedFromUser", roleName, targetUser.Name);
        PermissionsModule.AuditInstance.Log("musr.tempunassign", $"Role '{roleName}' was temporarily unassigned from user '{targetUser.Name}'",
            [$"anvilperms.targetuser:{targetUser.Name}", $"anvilperms.role:{roleName}", "anvilperms", "info"], user.Name, new()
        {
            {"user", targetUser.Name},
            {"role", roleName}
        });
    }

    [Command(["musr addgroup"], "amethyst.desc.permissions.musr.addgroup")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.musr.addgroup")]
    [CommandSyntax("en-US", "<user>", "<group>")]
    [CommandSyntax("ru-RU", "<пользователь>", "<группа>")]
    public static void UserAddGroup(IAmethystUser user, CommandInvokeContext ctx, string userName, string groupName)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(groupName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userAndGroupRequired");
            return;
        }
        
        if (user.Permissions.HasPermission($"anvil.float.assigngroup<{groupName}>") != PermissionAccess.HasPermission && !user.Commands.Repositories.Contains("root"))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cantAssignGroup");
            ctx.Messages.ReplyError("amethyst.reply.permissions.requiredPermissionIs", $"anvil.personal.assigngroup<{groupName}>");
            return;
        }

        var targetUser = ModuleStorage.Users.Find(userName);
        if (targetUser == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userNotFound", userName);
            return;
        }

        var group = ModuleStorage.Groups.Find(groupName);
        if (group == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupNotFound", groupName);
            return;
        }

        if (targetUser.Groups.Contains(group.Name))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupAlreadyInUser", group.Name, targetUser.Name);
            return;
        }

        targetUser.Groups.Add(group.Name);
        targetUser.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.groupAddedToUser", group.Name, targetUser.Name);
        PermissionsModule.AuditInstance.Log("musr.addgroup", $"Group '{group.Name}' was added to user '{targetUser.Name}'",
            [$"anvilperms.targetuser:{targetUser.Name}", $"anvilperms.group:{group.Name}", "anvilperms", "info"], user.Name, new()
        {
            {"user", targetUser.Name},
            {"group", group.Name}
        });
    }

    [Command(["musr removegroup"], "amethyst.desc.permissions.musr.removegroup")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.musr.removegroup")]
    [CommandSyntax("en-US", "<user>", "<group>")]
    [CommandSyntax("ru-RU", "<пользователь>", "<группа>")]
    public static void UserRemoveGroup(IAmethystUser user, CommandInvokeContext ctx, string userName, string groupName)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(groupName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userAndGroupRequired");
            return;
        }

        if (user.Permissions.HasPermission($"anvil.float.unassigngroup<{groupName}>") != PermissionAccess.HasPermission && !user.Commands.Repositories.Contains("root"))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cantUnassignGroup");
            ctx.Messages.ReplyError("amethyst.reply.permissions.requiredPermissionIs", $"anvil.personal.unassigngroup<{groupName}>");
            return;
        }

        var targetUser = ModuleStorage.Users.Find(userName);
        if (targetUser == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userNotFound", userName);
            return;
        }

        if (!targetUser.Groups.Contains(groupName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.groupNotInUser", groupName, targetUser.Name);
            return;
        }

        targetUser.Groups.Remove(groupName);
        targetUser.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.groupRemovedFromUser", groupName, targetUser.Name);
        PermissionsModule.AuditInstance.Log("musr.removegroup", $"Group '{groupName}' was removed from user '{targetUser.Name}'",
            [$"anvilperms.targetuser:{targetUser.Name}", $"anvilperms.group:{groupName}", "anvilperms", "info"], user.Name, new()
        {
            {"user", targetUser.Name},
            {"group", groupName}
        });
    }

    [Command(["musr addperm"], "amethyst.desc.permissions.musr.addperm")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.musr.addperm")]
    [CommandSyntax("en-US", "<user>", "<permission>")]
    [CommandSyntax("ru-RU", "<пользователь>", "<разрешение>")]
    public static void UserAddPermission(IAmethystUser user, CommandInvokeContext ctx, string userName, string permission)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(permission))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userAndPermissionRequired");
            return;
        }

        var targetUser = ModuleStorage.Users.Find(userName);
        if (targetUser == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userNotFound", userName);
            return;
        }

        if (targetUser.InternalGroup.Permissions.Contains(permission))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.permissionAlreadyInUser", permission, targetUser.Name);
            return;
        }

        targetUser.InternalGroup.Permissions.Add(permission);
        targetUser.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.permissionAddedToUser", permission, targetUser.Name);
        PermissionsModule.AuditInstance.Log("musr.addperm", $"Permission '{permission}' was added to user '{targetUser.Name}'",
            [$"anvilperms.targetuser:{targetUser.Name}", $"anvilperms.permission:{permission}", "anvilperms", "info"], user.Name, new()
        {
            {"user", targetUser.Name},
            {"permission", permission}
        });
    }

    [Command(["musr removeperm"], "amethyst.desc.permissions.musr.removeperm")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.musr.removeperm")]
    [CommandSyntax("en-US", "<user>", "<permission>")]
    [CommandSyntax("ru-RU", "<пользователь>", "<разрешение>")]
    public static void UserRemovePermission(IAmethystUser user, CommandInvokeContext ctx, string userName, string permission)
    {
        if (IsLocked)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.cannotModifyLocked");
            return;
        }

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(permission))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userAndPermissionRequired");
            return;
        }

        var targetUser = ModuleStorage.Users.Find(userName);
        if (targetUser == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userNotFound", userName);
            return;
        }

        if (!targetUser.InternalGroup.Permissions.Contains(permission))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.permissionNotInUser", permission, targetUser.Name);
            return;
        }

        targetUser.InternalGroup.Permissions.Remove(permission);
        targetUser.Save();

        ctx.Messages.ReplySuccess("amethyst.reply.permissions.permissionRemovedFromUser", permission, targetUser.Name);
        PermissionsModule.AuditInstance.Log("musr.removeperm", $"Permission '{permission}' was removed from user '{targetUser.Name}'",
            [$"anvilperms.targetuser:{targetUser.Name}", $"anvilperms.permission:{permission}", "anvilperms", "info"], user.Name, new()
        {
            {"user", targetUser.Name},
            {"permission", permission}
        });
    }

    [Command(["musr listperm"], "amethyst.desc.permissions.musr.listperm")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.musr.info")]
    [CommandSyntax("en-US", "<user>", "[page]")]
    [CommandSyntax("ru-RU", "<пользователь>", "[страница]")]
    public static void UserListPermissions(IAmethystUser user, CommandInvokeContext ctx, string userName, int page = 0)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userNameRequired");
            return;
        }

        var targetUser = ModuleStorage.Users.Find(userName);
        if (targetUser == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userNotFound", userName);
            return;
        }

        var permissions = targetUser.InternalGroup.Permissions;
        if (permissions.Count == 0)
        {
            ctx.Messages.ReplyInfo("amethyst.reply.permissions.noPermissionsInUser", targetUser.Name);
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(permissions);
        ctx.Messages.ReplyPage(pages, "amethyst.reply.permissions.userPermissionsList", null, null, false, page);
    }

    [Command(["musr listgroups"], "amethyst.desc.permissions.musr.listgroups")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.permissions.musr.info")]
    [CommandSyntax("en-US", "<user>", "[page]")]
    [CommandSyntax("ru-RU", "<пользователь>", "[страница]")]
    public static void UserListGroups(IAmethystUser user, CommandInvokeContext ctx, string userName, int page = 0)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userNameRequired");
            return;
        }

        var targetUser = ModuleStorage.Users.Find(userName);
        if (targetUser == null)
        {
            ctx.Messages.ReplyError("amethyst.reply.permissions.userNotFound", userName);
            return;
        }

        if (targetUser.Groups.Count == 0)
        {
            ctx.Messages.ReplyInfo("amethyst.reply.permissions.noGroupsInUser", targetUser.Name);
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(targetUser.Groups);
        ctx.Messages.ReplyPage(pages, "amethyst.reply.permissions.userGroupsList", null, null, false, page);
    }
}