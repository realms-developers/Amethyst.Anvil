using Amethyst.Network;
using Amethyst.Network.Handling.Base;
using Amethyst.Network.Handling.Packets.Handshake;
using Amethyst.Network.Packets;
using Amethyst.Network.Utilities;
using Amethyst.Server.Entities.Players;
using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Base.Permissions;
using Terraria;
using Terraria.DataStructures;

namespace Anvil.Regions.Network;

public sealed class ProtectionHandler : INetworkHandler
{
    public string Name => "net.anvil.ProtectionHandler";

    public void Load()
    {
        NetworkManager.AddSecurityHandler<WorldTileInteract>(OnTileInteract);
        NetworkManager.AddSecurityHandler<WorldPaintTile>(OnTilePaintTile);
        NetworkManager.AddSecurityHandler<WorldPaintWall>(OnTilePaintWall);
        NetworkManager.AddSecurityHandler<WorldTileRectangle>(OnTileRectangle);
        NetworkManager.AddSecurityHandler<WorldWiringHitSwitch>(OnTileWiringHitSwitch);
        NetworkManager.AddSecurityHandler<WorldLockSomething>(OnTileLockSomething);
        NetworkManager.AddSecurityHandler<WorldMassWireOperation>(OnTileMassWireOperation);
        NetworkManager.AddSecurityHandler<WorldPlaceObject>(OnTilePlaceObject);
        NetworkManager.AddSecurityHandler<WorldToggleGemLock>(OnTileToggleGemLock);
        NetworkManager.AddSecurityHandler<WorldAddLiquid>(OnTileAddLiquid);

        NetworkManager.AddSecurityHandler<TENewOrKill>(OnTENewOrKill);
        NetworkManager.AddSecurityHandler<TEPlaceEntity>(OnTEPlaceEntity);
        NetworkManager.AddSecurityHandler<TETryPlaceItemDisplayDoll>(OnTryPlaceItemDisplayDoll);
        NetworkManager.AddSecurityHandler<TETryPlaceItemFoodPlatter>(OnTryPlaceItemFoodPlatter);
        NetworkManager.AddSecurityHandler<TETryPlaceItemHatRack>(OnTryPlaceItemHatRack);
        NetworkManager.AddSecurityHandler<TETryPlaceItemItemFrame>(OnTryPlaceItemItemFrame);
        NetworkManager.AddSecurityHandler<TETryPlaceItemWeaponsRack>(OnTryPlaceItemWeaponsRack);

        NetworkManager.AddSecurityHandler<SignSync>(OnSignSync);

        NetworkManager.AddSecurityHandler<ChestInteract>(OnChestInteract);
        NetworkManager.AddSecurityHandler<ChestItemSync>(OnChestItemSync);
        NetworkManager.AddSecurityHandler<ChestRequestOpen>(OnChestRequestOpen);

        NetworkManager.AddDirectHandler(82, OnLiquidNetModule);
    }

    private void OnLiquidNetModule(PlayerEntity plr, ReadOnlySpan<byte> data, ref bool ignore)
    {
        if (plr.Phase != ConnectionPhase.Connected || plr.User == null)
        {
            ignore = true;
            return;
        }

        var reader = new FastPacketReader(data, 3);
        int count = reader.ReadUInt16();
        for (int i = 0; i < count; i++)
        {
            int num2 = reader.ReadInt32();
            reader.Skip(2);
            int x = num2 >> 16 & 65535;
            int y = num2 & 65535;

            ignore |= HandleXY(plr, x, y, PermissionType.Tile);
            if (ignore)
            {
                return;
            }
        }
    }

    private void OnChestRequestOpen(PlayerEntity plr, ref ChestRequestOpen packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        ignore = HandleXY(plr, packet.TileX, packet.TileY, PermissionType.ChestRead);
    }

    private void OnChestItemSync(PlayerEntity plr, ref ChestItemSync packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        if (packet.ChestIndex < 0 || packet.ChestIndex >= Main.chest.Length || plr.Phase != ConnectionPhase.Connected || plr.User == null)
        {
            ignore = true;
            return;
        }

        Chest chest = Main.chest[packet.ChestIndex];
        ignore |= chest == null || HandleXY(plr, chest.x, chest.y, PermissionType.ChestWrite);
    }

    private void OnChestInteract(PlayerEntity plr, ref ChestInteract packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        ignore |= HandleXY(plr, packet.ChestX, packet.ChestY, PermissionType.ChestWrite);
    }

    private void OnSignSync(PlayerEntity plr, ref SignSync packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        ignore |= HandleXY(plr, packet.SignX, packet.SignY, PermissionType.SignWrite);   
    }

    private void OnTryPlaceItemWeaponsRack(PlayerEntity plr, ref TETryPlaceItemWeaponsRack packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        ignore |= HandleXY(plr, packet.X, packet.Y, PermissionType.TileEntities);
    }

    private void OnTryPlaceItemItemFrame(PlayerEntity plr, ref TETryPlaceItemItemFrame packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        ignore |= HandleXY(plr, packet.X, packet.Y, PermissionType.TileEntities);
    }

    private void OnTryPlaceItemHatRack(PlayerEntity plr, ref TETryPlaceItemHatRack packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        if (plr.Phase != ConnectionPhase.Connected)
            return;

        if (TileEntity.ByID.TryGetValue(packet.TEIndex, out var te))
        {
            ignore |= HandleXY(plr, te.Position.X, te.Position.Y, PermissionType.TileEntities);
        }
        else
        {
            ignore = true;
        }
    }

    private void OnTryPlaceItemFoodPlatter(PlayerEntity plr, ref TETryPlaceItemFoodPlatter packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        ignore |= HandleXY(plr, packet.X, packet.Y, PermissionType.Tile);
    }

    private void OnTryPlaceItemDisplayDoll(PlayerEntity plr, ref TETryPlaceItemDisplayDoll packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        if (plr.Phase != ConnectionPhase.Connected)
            return;

        if (TileEntity.ByID.TryGetValue(packet.TEIndex, out var te))
        {
            ignore |= HandleXY(plr, te.Position.X, te.Position.Y, PermissionType.TileEntities);
        }
        else
        {
            ignore = true;
        }
    }

    private void OnTEPlaceEntity(PlayerEntity plr, ref TEPlaceEntity packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        ignore |= HandleXY(plr, packet.X, packet.Y, PermissionType.TileEntities);
    }

    private void OnTENewOrKill(PlayerEntity plr, ref TENewOrKill packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        if (plr.Phase != ConnectionPhase.Connected || plr.User == null)
        {
            ignore = true;
            return;
        }

        var reader = new FastPacketReader(rawPacket, 3);

        int teIndex = reader.ReadInt32();
        bool isCreate = reader.ReadBoolean();

        if (isCreate)
        {
            reader.Skip(1);
            short x = reader.ReadInt16();
            short y = reader.ReadInt16();

            ignore |= HandleXY(plr, x, y, PermissionType.TileEntities);
        }

        if (!isCreate && TileEntity.ByID.TryGetValue(teIndex, out var te))
        {
            ignore |= HandleXY(plr, te.Position.X, te.Position.Y, PermissionType.Tile);
        }
    }

    private void OnTileAddLiquid(PlayerEntity plr, ref WorldAddLiquid packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        ignore |= HandleXY(plr, packet.TileX, packet.TileY, PermissionType.Tile);
    }

    private void OnTileToggleGemLock(PlayerEntity plr, ref WorldToggleGemLock packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        ignore |= HandleXY(plr, packet.TileX, packet.TileY, PermissionType.Tile);
    }

    private void OnTilePlaceObject(PlayerEntity plr, ref WorldPlaceObject packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        ignore |= HandleXY(plr, packet.TileX, packet.TileY, PermissionType.Tile);
    }

    private void OnTileMassWireOperation(PlayerEntity plr, ref WorldMassWireOperation packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        int startX = Math.Min(packet.StartX, packet.EndX);
        int startY = Math.Min(packet.StartY, packet.EndY);
        int width = startX - Math.Max(packet.StartX, packet.EndX);
        int height = startY - Math.Max(packet.StartY, packet.EndY);

        if (width > 1000 || height > 1000 || plr.User == null || plr.Phase != ConnectionPhase.Connected)
        {
            ignore = true;
            return;
        }

        if (plr.User.Permissions.HasPermission(PermissionType.Tile, startX, startY, width, height) != PermissionAccess.HasPermission)
        {
            ignore = true;
            return;
        }
    }

    private void OnTileLockSomething(PlayerEntity plr, ref WorldLockSomething packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        ignore |= HandleXY(plr, packet.TileX, packet.TileY, PermissionType.Tile);
    }

    private void OnTileWiringHitSwitch(PlayerEntity plr, ref WorldWiringHitSwitch packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        ignore |= HandleXY(plr, packet.TileX, packet.TileY, PermissionType.Tile);
    }

    private void OnTileRectangle(PlayerEntity plr, ref WorldTileRectangle packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        if (packet.SizeX > 4 || packet.SizeY > 4 || plr.User == null || plr.Phase != ConnectionPhase.Connected)
        {
            ignore = true;
            return;
        }

        if (plr.User.Permissions.HasPermission(PermissionType.Tile, packet.StartX, packet.StartY, packet.SizeX, packet.SizeY) != PermissionAccess.HasPermission)
        {
            plr.SendRectangle(packet.StartX, packet.StartY, packet.SizeX, packet.SizeY);
            ignore = true;
            return;
        }
    }

    private void OnTilePaintWall(PlayerEntity plr, ref WorldPaintWall packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        ignore |= HandleXY(plr, packet.TileX, packet.TileY);
    }

    private void OnTilePaintTile(PlayerEntity plr, ref WorldPaintTile packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        ignore |= HandleXY(plr, packet.TileX, packet.TileY);
    }

    private void OnTileInteract(PlayerEntity plr, ref WorldTileInteract packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        if (plr.Phase != ConnectionPhase.Connected || plr.User == null)
        {
            ignore = true;
            return;
        }

        var ext = plr.User.Extensions.GetExtension("anvil.region") as Working.Extensions.PlayerRegionExtension;
        if (ext != null && ext.Selection != null)
        {
            return;
        }

        ignore |= HandleXY(plr, packet.TileX, packet.TileY);
    }

    private bool HandleXY(PlayerEntity plr, int x, int y, PermissionType type = PermissionType.Tile)
    {
        if (plr.Phase != ConnectionPhase.Connected || plr.User == null)
        {
            return false;
        }

        if (plr.User.Permissions.HasPermission(PermissionType.Tile, x, y) != PermissionAccess.HasPermission)
        {
            plr.SendRectangle(x, y, 4, 4);
            return true;
        }

        return false;
    }

    public void Unload()
    {
        NetworkManager.RemoveHandler<WorldTileInteract>(OnTileInteract);
        NetworkManager.RemoveHandler<WorldPaintTile>(OnTilePaintTile);
        NetworkManager.RemoveHandler<WorldPaintWall>(OnTilePaintWall);
        NetworkManager.RemoveHandler<WorldTileRectangle>(OnTileRectangle);
        NetworkManager.RemoveHandler<WorldWiringHitSwitch>(OnTileWiringHitSwitch);
        NetworkManager.RemoveHandler<WorldLockSomething>(OnTileLockSomething);
        NetworkManager.RemoveHandler<WorldMassWireOperation>(OnTileMassWireOperation);
        NetworkManager.RemoveHandler<WorldPlaceObject>(OnTilePlaceObject);
        NetworkManager.RemoveHandler<WorldToggleGemLock>(OnTileToggleGemLock);
        NetworkManager.RemoveHandler<WorldAddLiquid>(OnTileAddLiquid);

        NetworkManager.RemoveHandler<TENewOrKill>(OnTENewOrKill);
        NetworkManager.RemoveHandler<TEPlaceEntity>(OnTEPlaceEntity);
        NetworkManager.RemoveHandler<TETryPlaceItemDisplayDoll>(OnTryPlaceItemDisplayDoll);
        NetworkManager.RemoveHandler<TETryPlaceItemFoodPlatter>(OnTryPlaceItemFoodPlatter);
        NetworkManager.RemoveHandler<TETryPlaceItemHatRack>(OnTryPlaceItemHatRack);
        NetworkManager.RemoveHandler<TETryPlaceItemItemFrame>(OnTryPlaceItemItemFrame);
        NetworkManager.RemoveHandler<TETryPlaceItemWeaponsRack>(OnTryPlaceItemWeaponsRack);

        NetworkManager.RemoveHandler<SignSync>(OnSignSync);

        NetworkManager.RemoveHandler<ChestInteract>(OnChestInteract);
        NetworkManager.RemoveHandler<ChestItemSync>(OnChestItemSync);
        NetworkManager.RemoveHandler<ChestRequestOpen>(OnChestRequestOpen);

        NetworkManager.RemoveDirectHandler(82, OnLiquidNetModule);
    }
}
