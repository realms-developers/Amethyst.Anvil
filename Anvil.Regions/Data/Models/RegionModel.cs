using Amethyst.Storages.Mongo;
using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Base.Permissions;

namespace Anvil.Regions.Data.Models;

public sealed class RegionModel : DataModel
{
    public RegionModel(string name) : base(name)
    {
    }

    public int X { get; set; }
    public int Y { get; set; }
    public int X2 { get; set; }
    public int Y2 { get; set; }

    public int Z { get; set; }

    public string? ServerName { get; set; }

    public List<string> Tags { get; set; } = new();

    public List<RegionMember> Members { get; set; } = new();
    public List<RegionMember> Roles { get; set; } = new();

    public List<string> EnterCommands { get; set; } = new();
    public List<string> ExitCommands { get; set; } = new();
    public List<string> StayingCommands { get; set; } = new();

    public RegionMemberRank GetHighestUserRank(IAmethystUser user)
    {
        RegionMemberRank rank = RegionMemberRank.Member;

        foreach (RegionMember member in Members)
        {
            if (member.Name == user.Name)
            {
                if (member.Rank > rank)
                    rank = member.Rank;

                break;
            }
        }

        foreach (RegionMember role in Roles)
        {
            if (user.Permissions.HasPermission($"hasrole<{role.Name}>") == PermissionAccess.HasPermission)
            {
                if (role.Rank > rank)
                    rank = role.Rank;

                break;
            }
        }

        return rank;
    }   

    public override void Save()
    {
        ModuleStorage.Regions.Save(this);
    }

    public override void Remove()
    {
        ModuleStorage.Regions.Remove(Name);
    }
}