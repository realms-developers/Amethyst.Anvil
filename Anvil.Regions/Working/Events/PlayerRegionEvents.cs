using Amethyst;
using Amethyst.Hooks;
using Amethyst.Network.Handling.Packets.Handshake;
using Amethyst.Server.Entities;
using Anvil.Regions.Data.Models;
using Anvil.Regions.Working.Extensions;
using Anvil.Regions.Working.Users;

namespace Anvil.Regions.Working.Events;

public static class PlayerRegionEvents
{
    internal static Timer? UpdateTimer;
    internal static ExecutorUser User = new ExecutorUser();

    internal static void Initialize()
    {
        HookRegistry.RegisterHook<PlayerRegionEnterArgs>(false, false);
        HookRegistry.RegisterHook<PlayerRegionLeaveArgs>(false, false);
        HookRegistry.RegisterHook<PlayerRegionStayingArgs>(false, false);

        UpdateTimer = new Timer(UpdateRegions, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(250));
    }

    private static void UpdateRegions(object? state)
    {
        foreach (var plr in EntityTrackers.Players)
        {
            if (plr.Phase != ConnectionPhase.Connected || plr.User == null)
            {
                continue;
            }

            PlayerRegionExtension? ext = plr.User.Extensions.GetExtension("anvil.region") as PlayerRegionExtension;
            if (ext == null)
            {
                continue;
            }

            try
            {
                RegionModel? region = RegionsModule.Regions.FirstOrDefault(p => p.X <= (plr.Position.X / 16) && p.X2 >= (plr.Position.X / 16) && p.Y <= (plr.Position.Y / 16) && p.Y2 >= (plr.Position.Y / 16));

                if (region != ext.CurrentRegion)
                {
                    if (ext.CurrentRegion != null)
                    {
                        foreach (var cmd in ext.CurrentRegion.ExitCommands)
                        {
                            User.Commands.RunCommand(cmd.Replace("$PLAYER_ID$", plr.Index.ToString())
                                                        .Replace("$PLAYER_NAME$", plr.Name)
                                                        .Replace("$REGION_NAME$", ext.CurrentRegion.Name));
                        }

                        HookRegistry.GetHook<PlayerRegionLeaveArgs>().Invoke(new PlayerRegionLeaveArgs(plr.User, ext.CurrentRegion));
                    }

                    ext.CurrentRegion = region;

                    if (region != null)
                    {
                        foreach (var cmd in region.EnterCommands)
                        {
                            User.Commands.RunCommand(cmd.Replace("$PLAYER_ID$", plr.Index.ToString())
                                                        .Replace("$PLAYER_NAME$", plr.Name)
                                                        .Replace("$REGION_NAME$", region.Name));
                        }

                        HookRegistry.GetHook<PlayerRegionEnterArgs>().Invoke(new PlayerRegionEnterArgs(plr.User, region));
                    }
                }
                else if (ext.CurrentRegion != null)
                {
                    foreach (var cmd in ext.CurrentRegion.StayingCommands)
                    {
                        User.Commands.RunCommand(cmd.Replace("$PLAYER_ID$", plr.Index.ToString())
                                                    .Replace("$PLAYER_NAME$", plr.Name)
                                                    .Replace("$REGION_NAME$", ext.CurrentRegion.Name));
                    }

                    HookRegistry.GetHook<PlayerRegionStayingArgs>().Invoke(new PlayerRegionStayingArgs(plr.User, ext.CurrentRegion));
                }
            }
            catch (Exception ex)
            {
                AmethystLog.System.Critical(nameof(PlayerRegionEvents), $"Error while updating player region for {plr.User.Name}: {ex}");
                continue;
            }
        }
    }
}