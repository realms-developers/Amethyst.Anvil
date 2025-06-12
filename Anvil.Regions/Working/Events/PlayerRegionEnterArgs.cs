using Amethyst.Systems.Users.Players;
using Anvil.Regions.Data.Models;

namespace Anvil.Regions.Working.Events;

public sealed class PlayerRegionEnterArgs
{
    public PlayerRegionEnterArgs(PlayerUser user, RegionModel region)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
        Region = region ?? throw new ArgumentNullException(nameof(region));
    }
    
    public PlayerUser User { get; }
    public RegionModel Region { get; }
}