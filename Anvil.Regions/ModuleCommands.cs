using Amethyst.Systems.Commands.Base;
using Amethyst.Systems.Commands.Dynamic.Attributes;
using Amethyst.Systems.Users.Base.Permissions;
using Amethyst.Systems.Users.Players;
using Amethyst.Text;
using Anvil.Regions.Data;
using Anvil.Regions.Data.Models;
using Anvil.Regions.Working.Extensions;

namespace Anvil.Regions;

public static class ModuleCommands
{
    [Command(["rg select"], "amethyst.desc.region.createSelection")]
    [CommandPermission("anvil.regions.select")]
    [CommandRepository("shared")]
    public static void RegionSelect(PlayerUser user, CommandInvokeContext ctx)
    {
        var extension = user.Extensions.GetExtension("anvil.region") as PlayerRegionExtension;
        if (extension == null)
        {
            user.Messages.ReplyError("anvil.regions.extensionNotLoaded");
            return;
        }

        extension.Selection = new RegionSelection();
        user.Messages.ReplyInfo("anvil.regions.nowHitBlockToSetPoint1");
    }

    [Command(["rg sdefine"], "amethyst.desc.region.selectionDefine")]
    [CommandPermission("anvil.regions.define")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<name>")]
    [CommandSyntax("ru-RU", "<имя>")]
    public static void RegionDefine(PlayerUser user, CommandInvokeContext ctx, string name)
    {
        var extension = user.Extensions.GetExtension("anvil.region") as PlayerRegionExtension;
        if (extension == null)
        {
            user.Messages.ReplyError("anvil.regions.extensionNotLoaded");
            return;
        }

        if (extension.Selection == null || !extension.Selection.PointsSet)
        {
            user.Messages.ReplyError("anvil.regions.selectionNotDefined");
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            user.Messages.ReplyError("anvil.regions.invalidName");
            return;
        }

        if (ModuleStorage.Regions.Find(name) != null)
        {
            user.Messages.ReplyError("anvil.regions.regionAlreadyExists", name);
            return;
        }

        RegionModel model = new RegionModel(name)
        {
            X = extension.Selection.X,
            Y = extension.Selection.Y,
            X2 = extension.Selection.X2,
            Y2 = extension.Selection.Y2,
            Members = [new RegionMember()
            {
                Name = user.Name,
                Rank = RegionMemberRank.Admin
            }]
        };

        model.Save();

        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionDefined", name);

        RegionsModule.AuditInstance.Log(
            "rg.define", $"Region '{name}' defined by {user.Name} at ({model.X}, {model.Y}) to ({model.X2}, {model.Y2}).", [$"region:{name}"], user.Name, new()
            {
                { "region", name },
                { "x", model.X.ToString() },
                { "y", model.Y.ToString() },
                { "x2", model.X2.ToString() },
                { "y2", model.Y2.ToString() }
            });
    }

    [Command(["rg tdefine"], "amethyst.desc.region.targetBoundsDefine")]
    [CommandPermission("anvil.regions.targetdefine")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<name>", "<x>", "<y>", "<x2>", "<y2>")]
    [CommandSyntax("ru-RU", "<имя>", "<x>", "<y>", "<x2>", "<y2>")]
    public static void RegionTargetDefine(PlayerUser user, CommandInvokeContext ctx, string name, int x, int y, int x2, int y2)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            user.Messages.ReplyError("anvil.regions.invalidName");
            return;
        }

        if (ModuleStorage.Regions.Find(name) != null)
        {
            user.Messages.ReplyError("anvil.regions.regionAlreadyExists", name);
            return;
        }

        RegionModel model = new RegionModel(name)
        {
            X = x,
            Y = y,
            X2 = x2,
            Y2 = y2,
            Members = [new RegionMember()
            {
                Name = user.Name,
                Rank = RegionMemberRank.Admin
            }]
        };

        model.Save();

        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionDefined", name);

        RegionsModule.AuditInstance.Log(
            "rg.tdefine", $"Region '{name}' defined by {user.Name} at ({model.X}, {model.Y}) to ({model.X2}, {model.Y2}).", [$"region:{name}"], user.Name, new()
            {
                { "region", name },
                { "x", model.X.ToString() },
                { "y", model.Y.ToString() },
                { "x2", model.X2.ToString() },
                { "y2", model.Y2.ToString() }
            });
    }

    [Command(["rg remove", "rg rm"], "amethyst.desc.region.delete")]
    [CommandPermission("anvil.regions.delete")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>")]
    [CommandSyntax("ru-RU", "<регион>")]
    public static void RegionDelete(PlayerUser user, CommandInvokeContext ctx, string regionName)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        region.Remove();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionDeleted", regionName);

        RegionsModule.AuditInstance.Log(
            "rg.delete", $"Region '{regionName}' deleted by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName }
            });
    }

    [Command(["rg addmember"], "amethyst.desc.region.addMember")]
    [CommandPermission("anvil.regions.addmember")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<member>", "[rank (0 - member, 1 - moderator, 2 - admin)]")]
    [CommandSyntax("ru-RU", "<регион>", "<участник>", "[ранг (0 - member, 1 - moderator, 2 - admin)]")]
    public static void RegionAddMember(PlayerUser user, CommandInvokeContext ctx, string regionName, string memberName, int rank = 0)
    {
        if (rank < 0 || rank > 2)
        {
            user.Messages.ReplyError("anvil.regions.invalidRank");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (region.GetHighestUserRank(user) <= (RegionMemberRank)rank)
        {
            user.Messages.ReplyError("anvil.regions.rankTooLow", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Moderator)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (region.Members.Any(p => p.Name == memberName))
        {
            user.Messages.ReplyError("anvil.regions.memberAlreadyExists", memberName);
            return;
        }

        region.Members.Add(new RegionMember()
        {
            Name = memberName,
            Rank = (RegionMemberRank)rank
        });

        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.memberAdded", memberName, regionName);

        RegionsModule.AuditInstance.Log(
            "rg.addmember", $"Member '{memberName}' added to region '{regionName}' by {user.Name}.", [$"region:{regionName}", $"member:{memberName}"], user.Name, new()
            {
                { "region", regionName },
                { "member", memberName },
                { "rank", rank.ToString() },
                { "rank_id", Convert.ToByte(rank).ToString() }
            });
    }

    [Command(["rg removemember"], "amethyst.desc.region.removeMember")]
    [CommandPermission("anvil.regions.removemember")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<member>")]
    [CommandSyntax("ru-RU", "<регион>", "<участник>")]
    public static void RegionRemoveMember(PlayerUser user, CommandInvokeContext ctx, string regionName, string memberName)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Moderator)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        RegionMember? member = region.Members.FirstOrDefault(p => p.Name == memberName);
        if (member == null)
        {
            user.Messages.ReplyError("anvil.regions.memberNotFound", memberName);
            return;
        }

        if (region.GetHighestUserRank(user) <= member.Rank)
        {
            user.Messages.ReplyError("anvil.regions.rankTooLow", regionName);
            return;
        }

        region.Members.Remove(member);
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.memberRemoved", memberName, regionName);

        RegionsModule.AuditInstance.Log(
            "rg.removemember", $"Member '{memberName}' removed from region '{regionName}' by {user.Name}.", [$"region:{regionName}", $"member:{memberName}"], user.Name, new()
            {
                { "region", regionName },
                { "member", memberName }
            });
    }

    [Command(["rg setmemberrank"], "amethyst.desc.region.setMemberRank")]
    [CommandPermission("anvil.regions.setrank")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<member>", "<rank (0 - member, 1 - moderator, 2 - admin)>")]
    [CommandSyntax("ru-RU", "<регион>", "<участник>", "<ранг (0 - member, 1 - moderator, 2 - admin)>")]
    public static void RegionSetRank(PlayerUser user, CommandInvokeContext ctx, string regionName, string memberName, int rank)
    {
        if (rank < 0 || rank > 2)
        {
            user.Messages.ReplyError("anvil.regions.invalidRank");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (region.GetHighestUserRank(user) <= (RegionMemberRank)rank)
        {
            user.Messages.ReplyError("anvil.regions.rankTooLow", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        RegionMember? member = region.Members.FirstOrDefault(p => p.Name == memberName);
        if (member == null)
        {
            user.Messages.ReplyError("anvil.regions.memberNotFound", memberName);
            return;
        }

        member.Rank = (RegionMemberRank)rank;
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.memberRankSet", memberName, regionName, rank);

        RegionsModule.AuditInstance.Log(
            "rg.setrank", $"Rank of member '{memberName}' in region '{regionName}' set to {rank} by {user.Name}.", [$"region:{regionName}", $"member:{memberName}"], user.Name, new()
            {
                { "region", regionName },
                { "member", memberName },
                { "rank", rank.ToString() },
                { "rank_id", Convert.ToByte(rank).ToString() }
            });
    }

    [Command(["rg addrole"], "amethyst.desc.region.addRole")]
    [CommandPermission("anvil.regions.addrole")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<role>", "[rank (0 - member, 1 - moderator, 2 - admin)]")]
    [CommandSyntax("ru-RU", "<регион>", "<роль>", "[ранг (0 - member, 1 - moderator, 2 - admin)]")]
    public static void RegionAddRole(PlayerUser user, CommandInvokeContext ctx, string regionName, string roleName, int rank = 0)
    {
        if (rank < 0 || rank > 2)
        {
            user.Messages.ReplyError("anvil.regions.invalidRank");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (region.Roles.Any(p => p.Name == roleName))
        {
            user.Messages.ReplyError("anvil.regions.roleAlreadyExists", roleName);
            return;
        }

        region.Roles.Add(new RegionMember()
        {
            Name = roleName,
            Rank = (RegionMemberRank)rank
        });

        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.roleAdded", roleName, regionName);

        RegionsModule.AuditInstance.Log(
            "rg.addrole", $"Role '{roleName}' added to region '{regionName}' by {user.Name}.", [$"region:{regionName}", $"role:{roleName}"], user.Name, new()
            {
                { "region", regionName },
                { "role", roleName },
                { "rank", rank.ToString() },
                { "rank_id", Convert.ToByte(rank).ToString() }
            });
    }

    [Command(["rg removerole"], "amethyst.desc.region.removeRole")]
    [CommandPermission("anvil.regions.removerole")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<role>")]
    [CommandSyntax("ru-RU", "<регион>", "<роль>")]
    public static void RegionRemoveRole(PlayerUser user, CommandInvokeContext ctx, string regionName, string roleName)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        RegionMember? role = region.Roles.FirstOrDefault(p => p.Name == roleName);
        if (role == null)
        {
            user.Messages.ReplyError("anvil.regions.roleNotFound", roleName);
            return;
        }

        region.Roles.Remove(role);
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.roleRemoved", roleName, regionName);

        RegionsModule.AuditInstance.Log(
            "rg.removerole", $"Role '{roleName}' removed from region '{regionName}' by {user.Name}.", [$"region:{regionName}", $"role:{roleName}"], user.Name, new()
            {
                { "region", regionName },
                { "role", roleName }
            });
    }

    [Command(["rg setrolerank"], "amethyst.desc.region.setRoleRank")]
    [CommandPermission("anvil.regions.setrolerank")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<role>", "<rank (0 - member, 1 - moderator, 2 - admin)>")]
    [CommandSyntax("ru-RU", "<регион>", "<роль>", "<ранг (0 - member, 1 - moderator, 2 - admin)>")]
    public static void RegionSetRoleRank(PlayerUser user, CommandInvokeContext ctx, string regionName, string roleName, int rank)
    {
        if (rank < 0 || rank > 2)
        {
            user.Messages.ReplyError("anvil.regions.invalidRank");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        RegionMember? role = region.Roles.FirstOrDefault(p => p.Name == roleName);
        if (role == null)
        {
            user.Messages.ReplyError("anvil.regions.roleNotFound", roleName);
            return;
        }

        role.Rank = (RegionMemberRank)rank;
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.roleRankSet", roleName, regionName, rank);

        RegionsModule.AuditInstance.Log(
            "rg.setrolerank", $"Rank of role '{roleName}' in region '{regionName}' set to {rank} by {user.Name}.", [$"region:{regionName}", $"role:{roleName}"], user.Name, new()
            {
                { "region", regionName },
                { "role", roleName },
                { "rank", rank.ToString() },
                { "rank_id", Convert.ToByte(rank).ToString() }
            });
    }

    [Command(["rg listmembers"], "amethyst.desc.region.listMembers")]
    [CommandPermission("anvil.regions.listmembers")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "[page]")]
    [CommandSyntax("ru-RU", "<регион>", "[страница]")]
    public static void RegionListMembers(PlayerUser user, CommandInvokeContext ctx, string regionName, int page = 0)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Moderator)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (region.Members.Count == 0)
        {
            user.Messages.ReplyInfo("anvil.regions.noMembersDefined");
            return;
        }

        var pages = PagesCollection.AsListPage(region.Members.Select(p => $"{p.Name} ({p.Rank})"));
        user.Messages.ReplyPage(pages, "anvil.regions.memberListHeader", null, null, true, page);
    }

    [Command(["rg listroles"], "amethyst.desc.region.listRoles")]
    [CommandPermission("anvil.regions.listroles")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "[page]")]
    [CommandSyntax("ru-RU", "<регион>", "[страница]")]
    public static void RegionListRoles(PlayerUser user, CommandInvokeContext ctx, string regionName, int page = 0)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (region.Roles.Count == 0)
        {
            user.Messages.ReplyInfo("anvil.regions.noRolesDefined");
            return;
        }

        var pages = PagesCollection.AsListPage(region.Roles.Select(p => $"{p.Name} ({p.Rank})"));
        user.Messages.ReplyPage(pages, "anvil.regions.roleListHeader", null, null, true, page);
    }

    [Command(["rg list"], "amethyst.desc.region.list")]
    [CommandPermission("anvil.regions.list")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "[page]")]
    [CommandSyntax("ru-RU", "[страница]")]
    public static void RegionList(PlayerUser user, CommandInvokeContext ctx, int page = 0)
    {
        var regions = ModuleStorage.Regions.FindAll();
        if (regions.Count() == 0)
        {
            user.Messages.ReplyInfo("anvil.regions.noRegionsDefined");
            return;
        }

        var pages = PagesCollection.AsListPage(regions.Select(p => p.Name));
        user.Messages.ReplyPage(pages, "anvil.regions.regionListHeader", null, null, true, page);
    }

    [Command(["rg addtag"], "amethyst.desc.region.addTag")]
    [CommandPermission("anvil.regions.addtag")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<tag>")]
    [CommandSyntax("ru-RU", "<регион>", "<тег>")]
    public static void RegionAddTag(PlayerUser user, CommandInvokeContext ctx, string regionName, string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            user.Messages.ReplyError("anvil.regions.invalidTag");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (region.Tags.Contains(tag))
        {
            user.Messages.ReplyError("anvil.regions.tagAlreadyExists", tag);
            return;
        }

        region.Tags.Add(tag);
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.tagAdded", tag, regionName);

        RegionsModule.AuditInstance.Log(
            "rg.addtag", $"Tag '{tag}' added to region '{regionName}' by {user.Name}.", [$"region:{regionName}", $"rg_tag:{tag}"], user.Name, new()
            {
                { "region", regionName },
                { "rg_tag", tag }
            });
    }

    [Command(["rg removetag"], "amethyst.desc.region.removeTag")]
    [CommandPermission("anvil.regions.removetag")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<tag>")]
    [CommandSyntax("ru-RU", "<регион>", "<тег>")]
    public static void RegionRemoveTag(PlayerUser user, CommandInvokeContext ctx, string regionName, string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            user.Messages.ReplyError("anvil.regions.invalidTag");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (!region.Tags.Contains(tag))
        {
            user.Messages.ReplyError("anvil.regions.tagNotFound", tag);
            return;
        }

        region.Tags.Remove(tag);
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.tagRemoved", tag, regionName);

        RegionsModule.AuditInstance.Log(
            "rg.removetag", $"Tag '{tag}' removed from region '{regionName}' by {user.Name}.", [$"region:{regionName}", $"rg_tag:{tag}"], user.Name, new()
            {
                { "region", regionName },
                { "rg_tag", tag }
            });
    }

    [Command(["rg taglist"], "amethyst.desc.region.tagList")]
    [CommandPermission("anvil.regions.taglist")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "[page]")]
    [CommandSyntax("ru-RU", "<регион>", "[страница]")]
    public static void RegionTagList(PlayerUser user, CommandInvokeContext ctx, string regionName, int page = 0)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Moderator)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (region.Tags.Count == 0)
        {
            user.Messages.ReplyInfo("anvil.regions.noTagsDefined");
            return;
        }

        var pages = PagesCollection.AsListPage(region.Tags);
        user.Messages.ReplyPage(pages, "anvil.regions.tagListHeader", null, null, true, page);
    }

    [Command(["rg moveleft"], "amethyst.desc.region.moveLeft")]
    [CommandPermission("anvil.regions.moveleft")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<distance>")]
    [CommandSyntax("ru-RU", "<регион>", "<расстояние>")]
    public static void RegionMoveLeft(PlayerUser user, CommandInvokeContext ctx, string regionName, int distance)
    {
        if (distance <= 0)
        {
            user.Messages.ReplyError("anvil.regions.invalidDistance");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        region.X -= distance;
        region.X2 -= distance;
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionMovedLeft", regionName, distance);

        RegionsModule.AuditInstance.Log(
            "rg.moveleft", $"Region '{regionName}' moved left by {distance} blocks by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName },
                { "distance", distance.ToString() }
            });
    }

    [Command(["rg moveright"], "amethyst.desc.region.moveRight")]
    [CommandPermission("anvil.regions.moveright")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<distance>")]
    [CommandSyntax("ru-RU", "<регион>", "<расстояние>")]
    public static void RegionMoveRight(PlayerUser user, CommandInvokeContext ctx, string regionName, int distance)
    {
        if (distance <= 0)
        {
            user.Messages.ReplyError("anvil.regions.invalidDistance");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        region.X += distance;
        region.X2 += distance;
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionMovedRight", regionName, distance);

        RegionsModule.AuditInstance.Log(
            "rg.moveright", $"Region '{regionName}' moved right by {distance} blocks by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName },
                { "distance", distance.ToString() }
            });
    }

    [Command(["rg moveup"], "amethyst.desc.region.moveUp")]
    [CommandPermission("anvil.regions.moveup")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<distance>")]
    [CommandSyntax("ru-RU", "<регион>", "<расстояние>")]
    public static void RegionMoveUp(PlayerUser user, CommandInvokeContext ctx, string regionName, int distance)
    {
        if (distance <= 0)
        {
            user.Messages.ReplyError("anvil.regions.invalidDistance");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        region.Y += distance;
        region.Y2 += distance;
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionMovedUp", regionName, distance);

        RegionsModule.AuditInstance.Log(
            "rg.moveup", $"Region '{regionName}' moved up by {distance} blocks by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName },
                { "distance", distance.ToString() }
            });
    }

    [Command(["rg movedown"], "amethyst.desc.region.moveDown")]
    [CommandPermission("anvil.regions.movedown")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<distance>")]
    [CommandSyntax("ru-RU", "<регион>", "<расстояние>")]
    public static void RegionMoveDown(PlayerUser user, CommandInvokeContext ctx, string regionName, int distance)
    {
        if (distance <= 0)
        {
            user.Messages.ReplyError("anvil.regions.invalidDistance");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        region.Y -= distance;
        region.Y2 -= distance;
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionMovedDown", regionName, distance);

        RegionsModule.AuditInstance.Log(
            "rg.movedown", $"Region '{regionName}' moved down by {distance} blocks by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName },
                { "distance", distance.ToString() }
            });
    }

    [Command(["rg resizeleft"], "amethyst.desc.region.resizeLeft")]
    [CommandPermission("anvil.regions.resizeleft")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<distance>")]
    [CommandSyntax("ru-RU", "<регион>", "<расстояние>")]
    public static void RegionResizeLeft(PlayerUser user, CommandInvokeContext ctx, string regionName, int distance)
    {
        if (distance <= 0)
        {
            user.Messages.ReplyError("anvil.regions.invalidDistance");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        region.X -= distance;
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionResizedLeft", regionName, distance);

        RegionsModule.AuditInstance.Log(
            "rg.resizeleft", $"Region '{regionName}' resized left by {distance} blocks by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName },
                { "distance", distance.ToString() }
            });
    }

    [Command(["rg resizeright"], "amethyst.desc.region.resizeRight")]
    [CommandPermission("anvil.regions.resizeright")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<distance>")]
    [CommandSyntax("ru-RU", "<регион>", "<расстояние>")]
    public static void RegionResizeRight(PlayerUser user, CommandInvokeContext ctx, string regionName, int distance)
    {
        if (distance <= 0)
        {
            user.Messages.ReplyError("anvil.regions.invalidDistance");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        region.X2 += distance;
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionResizedRight", regionName, distance);

        RegionsModule.AuditInstance.Log(
            "rg.resizeright", $"Region '{regionName}' resized right by {distance} blocks by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName },
                { "distance", distance.ToString() }
            });
    }

    [Command(["rg resizeup"], "amethyst.desc.region.resizeUp")]
    [CommandPermission("anvil.regions.resizeup")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<distance>")]
    [CommandSyntax("ru-RU", "<регион>", "<расстояние>")]
    public static void RegionResizeUp(PlayerUser user, CommandInvokeContext ctx, string regionName, int distance)
    {
        if (distance <= 0)
        {
            user.Messages.ReplyError("anvil.regions.invalidDistance");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        region.Y += distance;
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionResizedUp", regionName, distance);

        RegionsModule.AuditInstance.Log(
            "rg.resizeup", $"Region '{regionName}' resized up by {distance} blocks by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName },
                { "distance", distance.ToString() }
            });
    }

    [Command(["rg scale"], "amethyst.desc.region.scale")]
    [CommandPermission("anvil.regions.scale")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<scale>")]
    [CommandSyntax("ru-RU", "<регион>", "<масштаб>")]
    public static void RegionScale(PlayerUser user, CommandInvokeContext ctx, string regionName, float scale)
    {
        if (scale <= 0)
        {
            user.Messages.ReplyError("anvil.regions.invalidScale");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        int width = (int)((region.X2 - region.X) * scale);
        int height = (int)((region.Y2 - region.Y) * scale);

        region.X2 = region.X + width;
        region.Y2 = region.Y + height;
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionScaled", regionName, scale);

        RegionsModule.AuditInstance.Log(
            "rg.scale", $"Region '{regionName}' scaled by {scale} by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName },
                { "scale", scale.ToString() }
            });

    }

    [Command(["rg moveto"], "amethyst.desc.region.moveTo")]
    [CommandPermission("anvil.regions.moveto")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<x>", "<y>")]
    [CommandSyntax("ru-RU", "<регион>", "<x>", "<y>")]
    public static void RegionMoveTo(PlayerUser user, CommandInvokeContext ctx, string regionName, int x, int y)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        region.X2 = x + (region.X2 - region.X);
        region.Y2 = y + (region.Y2 - region.Y);
        region.X = x;
        region.Y = y;
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionMovedTo", regionName, x, y);

        RegionsModule.AuditInstance.Log(
            "rg.moveto", $"Region '{regionName}' moved to ({x}, {y}) by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName },
                { "x", x.ToString() },
                { "y", y.ToString() }
            });
    }

    [Command(["rg centeredmoveto"], "amethyst.desc.region.centeredMoveTo")]
    [CommandPermission("anvil.regions.centeredmoveto")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<x>", "<y>")]
    [CommandSyntax("ru-RU", "<регион>", "<x>", "<y>")]
    public static void RegionCenteredMoveTo(PlayerUser user, CommandInvokeContext ctx, string regionName, int x, int y)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        int width = (region.X2 - region.X) / 2;
        int height = (region.Y2 - region.Y) / 2;

        region.X = x - width;
        region.Y = y - height;
        region.X2 = x + width;
        region.Y2 = y + height;
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionCenteredMovedTo", regionName, x, y);

        RegionsModule.AuditInstance.Log(
            "rg.centeredmoveto", $"Region '{regionName}' centered moved to ({x}, {y}) by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName },
                { "x", x.ToString() },
                { "y", y.ToString() }
            });
    }

    [Command(["rg z"], "amethyst.desc.region.z")]
    [CommandPermission("anvil.regions.z")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<z>")]
    [CommandSyntax("ru-RU", "<регион>", "<z>")]
    public static void RegionZ(PlayerUser user, CommandInvokeContext ctx, string regionName, int z)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        region.Z = z;
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionZSet", regionName, z);

        RegionsModule.AuditInstance.Log(
            "rg.z", $"Region '{regionName}' Z coordinate set to {z} by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName },
                { "z", z.ToString() }
            });
    }

    [Command(["rg x"], "amethyst.desc.region.x")]
    [CommandPermission("anvil.regions.x")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<x>")]
    [CommandSyntax("ru-RU", "<регион>", "<x>")]
    public static void RegionX(PlayerUser user, CommandInvokeContext ctx, string regionName, int x)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        region.X = x;
        region.X2 = x + (region.X2 - region.X);
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionXSet", regionName, x);

        RegionsModule.AuditInstance.Log(
            "rg.x", $"Region '{regionName}' X coordinate set to {x} by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName },
                { "x", x.ToString() }
            });

    }

    [Command(["rg y"], "amethyst.desc.region.y")]
    [CommandPermission("anvil.regions.y")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<y>")]
    [CommandSyntax("ru-RU", "<регион>", "<y>")]
    public static void RegionY(PlayerUser user, CommandInvokeContext ctx, string regionName, int y)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        region.Y = y;
        region.Y2 = y + (region.Y2 - region.Y);
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionYSet", regionName, y);

        RegionsModule.AuditInstance.Log(
            "rg.y", $"Region '{regionName}' Y coordinate set to {y} by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName },
                { "y", y.ToString() }
            });
    }

    [Command(["rg x2"], "amethyst.desc.region.x2")]
    [CommandPermission("anvil.regions.x2")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<x2>")]
    [CommandSyntax("ru-RU", "<регион>", "<x2>")]
    public static void RegionX2(PlayerUser user, CommandInvokeContext ctx, string regionName, int x2)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        region.X2 = x2;
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionX2Set", regionName, x2);

        RegionsModule.AuditInstance.Log(
            "rg.x2", $"Region '{regionName}' X2 coordinate set to {x2} by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName },
                { "x2", x2.ToString() }
            });
    }

    [Command(["rg y2"], "amethyst.desc.region.y2")]
    [CommandPermission("anvil.regions.y2")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<y2>")]
    [CommandSyntax("ru-RU", "<регион>", "<y2>")]
    public static void RegionY2(PlayerUser user, CommandInvokeContext ctx, string regionName, int y2)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        region.Y2 = y2;
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.regionY2Set", regionName, y2);

        RegionsModule.AuditInstance.Log(
            "rg.y2", $"Region '{regionName}' Y2 coordinate set to {y2} by {user.Name}.", [$"region:{regionName}"], user.Name, new()
            {
                { "region", regionName },
                { "y2", y2.ToString() }
            });
    }

    [Command(["rg addentercmd"], "amethyst.desc.region.addEnterCommand")]
    [CommandPermission("anvil.regions.addentercmd")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<command>")]
    [CommandSyntax("ru-RU", "<регион>", "<команда>")]
    public static void RegionAddEnterCommand(PlayerUser user, CommandInvokeContext ctx, string regionName, string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            user.Messages.ReplyError("anvil.regions.invalidCommand");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (region.EnterCommands.Contains(command))
        {
            user.Messages.ReplyError("anvil.regions.commandAlreadyExists", command);
            return;
        }

        region.EnterCommands.Add(command);
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.enterCommandAdded", command, regionName);

        RegionsModule.AuditInstance.Log(
            "rg.addentercmd", $"Enter command '{command}' added to region '{regionName}' by {user.Name}.", [$"region:{regionName}", $"rg_enter_cmd:{command}"], user.Name, new()
            {
                { "region", regionName },
                { "rg_enter_cmd", command }
            });
    }

    [Command(["rg removeentercmd"], "amethyst.desc.region.removeEnterCommand")]
    [CommandPermission("anvil.regions.removeentercmd")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<command>")]
    [CommandSyntax("ru-RU", "<регион>", "<команда>")]
    public static void RegionRemoveEnterCommand(PlayerUser user, CommandInvokeContext ctx, string regionName, string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            user.Messages.ReplyError("anvil.regions.invalidCommand");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (!region.EnterCommands.Contains(command))
        {
            user.Messages.ReplyError("anvil.regions.commandNotFound", command);
            return;
        }

        region.EnterCommands.Remove(command);
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.enterCommandRemoved", command, regionName);

        RegionsModule.AuditInstance.Log(
            "rg.removeentercmd", $"Enter command '{command}' removed from region '{regionName}' by {user.Name}.", [$"region:{regionName}", $"rg_enter_cmd:{command}"], user.Name, new()
            {
                { "region", regionName },
                { "rg_enter_cmd", command }
            });
    }
    [Command(["rg listentercmds"], "amethyst.desc.region.listEnterCommands")]
    [CommandPermission("anvil.regions.listentercmds")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "[page]")]
    [CommandSyntax("ru-RU", "<регион>", "[страница]")]
    public static void RegionListEnterCommands(PlayerUser user, CommandInvokeContext ctx, string regionName, int page = 0)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Moderator)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (region.EnterCommands.Count == 0)
        {
            user.Messages.ReplyInfo("anvil.regions.noEnterCommandsDefined");
            return;
        }

        var pages = PagesCollection.AsPage(region.EnterCommands.Select(p => $"'{p}'"));
        user.Messages.ReplyPage(pages, "anvil.regions.enterCommandListHeader", null, null, true, page);
    }

    [Command(["rg addexitcmd"], "amethyst.desc.region.addExitCommand")]
    [CommandPermission("anvil.regions.addexitcmd")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<command>")]
    [CommandSyntax("ru-RU", "<регион>", "<команда>")]
    public static void RegionAddExitCommand(PlayerUser user, CommandInvokeContext ctx, string regionName, string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            user.Messages.ReplyError("anvil.regions.invalidCommand");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (region.ExitCommands.Contains(command))
        {
            user.Messages.ReplyError("anvil.regions.commandAlreadyExists", command);
            return;
        }

        region.ExitCommands.Add(command);
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.exitCommandAdded", command, regionName);

        RegionsModule.AuditInstance.Log(
            "rg.addexitcmd", $"Exit command '{command}' added to region '{regionName}' by {user.Name}.", [$"region:{regionName}", $"rg_exit_cmd:{command}"], user.Name, new()
            {
                { "region", regionName },
                { "rg_exit_cmd", command }
            });
    }

    [Command(["rg removeexitcmd"], "amethyst.desc.region.removeExitCommand")]
    [CommandPermission("anvil.regions.removeexitcmd")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<command>")]
    [CommandSyntax("ru-RU", "<регион>", "<команда>")]
    public static void RegionRemoveExitCommand(PlayerUser user, CommandInvokeContext ctx, string regionName, string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            user.Messages.ReplyError("anvil.regions.invalidCommand");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (!region.ExitCommands.Contains(command))
        {
            user.Messages.ReplyError("anvil.regions.commandNotFound", command);
            return;
        }

        region.ExitCommands.Remove(command);
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.exitCommandRemoved", command, regionName);

        RegionsModule.AuditInstance.Log(
            "rg.removeexitcmd", $"Exit command '{command}' removed from region '{regionName}' by {user.Name}.", [$"region:{regionName}", $"rg_exit_cmd:{command}"], user.Name, new()
            {
                { "region", regionName },
                { "rg_exit_cmd", command }
            });
    }

    [Command(["rg listexitcmds"], "amethyst.desc.region.listExitCommands")]
    [CommandPermission("anvil.regions.listexitcmds")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "[page]")]
    [CommandSyntax("ru-RU", "<регион>", "[страница]")]
    public static void RegionListExitCommands(PlayerUser user, CommandInvokeContext ctx, string regionName, int page = 0)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Moderator)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (region.ExitCommands.Count == 0)
        {
            user.Messages.ReplyInfo("anvil.regions.noExitCommandsDefined");
            return;
        }

        var pages = PagesCollection.AsPage(region.ExitCommands.Select(p => $"'{p}'"));
        user.Messages.ReplyPage(pages, "anvil.regions.exitCommandListHeader", null, null, true, page);
    }

    [Command(["rg addstayingcmd"], "amethyst.desc.region.addStayingCommand")]
    [CommandPermission("anvil.regions.addstayingcmd")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<command>")]
    [CommandSyntax("ru-RU", "<регион>", "<команда>")]
    public static void RegionAddStayingCommand(PlayerUser user, CommandInvokeContext ctx, string regionName, string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            user.Messages.ReplyError("anvil.regions.invalidCommand");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (region.StayingCommands.Contains(command))
        {
            user.Messages.ReplyError("anvil.regions.commandAlreadyExists", command);
            return;
        }

        region.StayingCommands.Add(command);
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.stayingCommandAdded", command, regionName);

        RegionsModule.AuditInstance.Log(
            "rg.addstayingcmd", $"Staying command '{command}' added to region '{regionName}' by {user.Name}.", [$"region:{regionName}", $"rg_staying_cmd:{command}"], user.Name, new()
            {
                { "region", regionName },
                { "rg_staying_cmd", command }
            });
    }

    [Command(["rg removestayingcmd"], "amethyst.desc.region.removeStayingCommand")]
    [CommandPermission("anvil.regions.removestayingcmd")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "<command>")]
    [CommandSyntax("ru-RU", "<регион>", "<команда>")]
    public static void RegionRemoveStayingCommand(PlayerUser user, CommandInvokeContext ctx, string regionName, string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            user.Messages.ReplyError("anvil.regions.invalidCommand");
            return;
        }

        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Admin)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (!region.StayingCommands.Contains(command))
        {
            user.Messages.ReplyError("anvil.regions.commandNotFound", command);
            return;
        }

        region.StayingCommands.Remove(command);
        region.Save();
        RegionsModule.ReloadRegions();
        user.Messages.ReplySuccess("anvil.regions.stayingCommandRemoved", command, regionName);

        RegionsModule.AuditInstance.Log(
            "rg.removestayingcmd", $"Staying command '{command}' removed from region '{regionName}' by {user.Name}.", [$"region:{regionName}", $"rg_staying_cmd:{command}"], user.Name, new()
            {
                { "region", regionName },
                { "rg_staying_cmd", command }
            });
    }

    [Command(["rg liststayingcmds"], "amethyst.desc.region.listStayingCommands")]
    [CommandPermission("anvil.regions.liststayingcmds")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<region>", "[page]")]
    [CommandSyntax("ru-RU", "<регион>", "[страница]")]
    public static void RegionListStayingCommands(PlayerUser user, CommandInvokeContext ctx, string regionName, int page = 0)
    {
        RegionModel? region = ModuleStorage.Regions.Find(regionName);
        if (region == null)
        {
            user.Messages.ReplyError("anvil.regions.regionNotFound", regionName);
            return;
        }

        if (user.Permissions.HasPermission("anvil.float.fullregionaccess") != PermissionAccess.HasPermission && region.GetHighestUserRank(user) < RegionMemberRank.Moderator)
        {
            user.Messages.ReplyError("anvil.regions.noPermissionToManageRegion", regionName);
            return;
        }

        if (region.StayingCommands.Count == 0)
        {
            user.Messages.ReplyInfo("anvil.regions.noStayingCommandsDefined");
            return;
        }

        var pages = PagesCollection.AsPage(region.StayingCommands.Select(p => $"'{p}'"));
        user.Messages.ReplyPage(pages, "anvil.regions.stayingCommandListHeader", null, null, true, page);
    }
}