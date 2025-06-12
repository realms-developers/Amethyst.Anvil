using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Base.Extensions;
using Amethyst.Systems.Users.Players;
using Anvil.Regions.Data.Models;

namespace Anvil.Regions.Working.Extensions;

public sealed class PlayerRegionExtension : IUserExtension
{
    public string Name => "anvil.region";

    public PlayerUser User { get; private set; } = null!;

    public RegionSelection? Selection { get; set; }
    public DateTime? LastPointSet { get; set; }
    public RegionModel? CurrentRegion { get; set; }

    public bool CanSetPoint()
    {
        if (LastPointSet == null)
        {
            LastPointSet = DateTime.UtcNow;
            return true;
        }

        var timeSinceLastSet = DateTime.UtcNow - LastPointSet.Value;
        if (timeSinceLastSet.TotalSeconds >= 1)
        {
            LastPointSet = DateTime.UtcNow;
            return true;
        }

        return false;
    }

    public void Load(IAmethystUser player)
    {
        if (player is not PlayerUser user)
            throw new ArgumentException("PlayerRegionExtension can only be loaded for PlayerUser instances.", nameof(player));

        User = user;
    }

    public void Unload(IAmethystUser player)
    {
    }
}