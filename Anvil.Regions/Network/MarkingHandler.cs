using Amethyst.Network;
using Amethyst.Network.Handling.Base;
using Amethyst.Network.Handling.Packets.Handshake;
using Amethyst.Network.Packets;
using Amethyst.Server.Entities.Players;
using Anvil.Regions.Working.Extensions;

namespace Anvil.Regions.Network;

public sealed class MarkingHandler : INetworkHandler
{
    public string Name => "net.anvil.MarkingHandler";

    public void Load()
    {
        NetworkManager.AddHandler<WorldTileInteract>(OnTileInteract);
    }

    private void OnTileInteract(PlayerEntity plr, ref WorldTileInteract packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        if (plr.Phase != ConnectionPhase.Connected || plr.User == null)
        {
            return;
        }

        var ext = plr.User.Extensions.GetExtension("anvil.region") as PlayerRegionExtension;
        if (ext == null)
        {
            return;
        }

        if (ext.Selection == null || ext.Selection.PointsSet || !ext.CanSetPoint())
        {
            return;
        }
        ignore = true;

        if (ext.Selection.Point1Set)
        {
            ext.Selection.SetPoint1(packet.TileX, packet.TileY);
            plr.User.Messages.ReplyInfo("anvil.regions.setpoint1", packet.TileX, packet.TileY);
        }
        else
        {
            ext.Selection.SetPoint2(packet.TileX, packet.TileY);
            plr.User.Messages.ReplyInfo("anvil.regions.setpoint2", packet.TileX, packet.TileY);
            plr.User.Messages.ReplyInfo("anvil.regions.useCommandToDefineRegion");
        }
    }

    public void Unload()
    {
        NetworkManager.RemoveHandler<WorldTileInteract>(OnTileInteract);
    }
}
