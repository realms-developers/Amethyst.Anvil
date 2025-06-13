using Amethyst;
using Amethyst.Essentials.Data.Models;
using Amethyst.Network.Structures;
using Amethyst.Server.Entities.Players;
using Amethyst.Systems.Commands.Base;
using Amethyst.Systems.Commands.Dynamic.Attributes;
using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Players;
using Amethyst.Text;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using static Amethyst.Localization;

namespace Anvil.Essentials;

public static class PluginCommands
{
    [Command(["spawn", "spawnpoint"], "anvil.essentials.teleportToSpawnpoint")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.player.teleportToSpawnpoint")]
    public static void TeleportToSpawnPointCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        user.Player.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);
        ctx.Messages.ReplySuccess("anvil.essentials.teleportToSpawnpoint.success");
    }

    [Command(["i", "item"], "anvil.essentials.item")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<item name ...>", "[amount]", "[prefix]")]
    [CommandSyntax("ru-RU", "<название предмета ...>", "[количество]", "[префикс]")]
    [CommandPermission("anvil.essentials.item")]
    public static void ItemCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        if (ctx.Args.Count() < 1)
        {
            ctx.Messages.ReplyError("anvil.essentials.item.syntax");
            return;
        }

        List<string> itemNameArgs = [ctx.Args[0]];
        for (int i = 1; i < ctx.Args.Length; i++)
        {
            if (int.TryParse(ctx.Args[i], out _))
            {
                break;
            }

            itemNameArgs.Add(ctx.Args[i]);
        }

        string itemName = string.Join(" ", itemNameArgs);
        int amount = -1;
        byte prefix = 0;
        if (ctx.Args.Length > itemNameArgs.Count && int.TryParse(ctx.Args[itemNameArgs.Count], out int parsedAmount))
        {
            amount = parsedAmount;
        }
        else if (ctx.Args.Length > itemNameArgs.Count)
        {
            ctx.Messages.ReplyError("anvil.essentials.invalidAmount");
            return;
        }
        if (ctx.Args.Length > itemNameArgs.Count + 1 && byte.TryParse(ctx.Args[itemNameArgs.Count + 1], out byte parsedPrefix))
        {
            prefix = parsedPrefix;
        }
        else if (ctx.Args.Length > itemNameArgs.Count + 1)
        {
            ctx.Messages.ReplyError("anvil.essentials.invalidPrefix");
            return;
        }

        if (string.IsNullOrEmpty(itemName))
        {
            ctx.Messages.ReplyError("anvil.essentials.invalidItemName");
            return;
        }

        NetItem? itemFromTag = Items.GetItemFromTag(itemName);
        List<ItemFindData> foundItems =
            int.TryParse(itemName, out int itemId) ? [new ItemFindData(itemId, Lang.GetItemNameValue(itemId))] :
            itemFromTag != null ? [new ItemFindData(itemFromTag.Value.ID, Lang.GetItemNameValue(itemFromTag.Value.ID))] :
            Items.FindItem(false, itemName);

        if (foundItems.Count == 0)
        {
            foundItems.AddRange(Items.FindItem(true, itemName));
        }

        if (foundItems.Count == 0)
        {
            ctx.Messages.ReplyError("anvil.essentials.item.notFound", itemName);
            return;
        }

        if (foundItems.Count > 1)
        {
            ctx.Messages.ReplyError("anvil.essentials.item.multipleFound", itemName);
            PagesCollection pages = PagesCollection.AsListPage(foundItems.Select(item => $"{item.Name} ({item.ItemID})").ToList());
            return;
        }

        user.Player.GiveItem(foundItems[0].ItemID, amount == -1 ? 9999 : amount, prefix);
        ctx.Messages.ReplySuccess("anvil.essentials.item.given", foundItems[0].Name, amount == -1 ? "9999" : amount.ToString(), prefix.ToString());
    }

    [Command(["godmode", "gm"], "anvil.essentials.godmode")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.essentials.godmode")]
    public static void GodModeCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        user.Player.SetGodMode(!user.Player.IsGodModeEnabled);
        if (user.Player.IsGodModeEnabled)
        {
            ctx.Messages.ReplySuccess("anvil.essentials.godmode.enabled");
        }
        else
        {
            ctx.Messages.ReplySuccess("anvil.essentials.godmode.disabled");
        }
    }

    [Command(["tp", "teleport"], "anvil.essentials.teleport")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<player>", "[to player]")]
    [CommandSyntax("ru-RU", "<игрок>", "[к игроку]")]
    [CommandPermission("anvil.essentials.teleport")]
    public static void TeleportCommand(IAmethystUser user, CommandInvokeContext ctx, PlayerEntity plr, PlayerEntity? toPlr = null)
    {
        if (user is not PlayerUser && toPlr == null)
        {
            ctx.Messages.ReplyError("anvil.essentials.teleport.notPlayer");
            return;
        }

        toPlr ??= ((PlayerUser)user).Player;

        toPlr.Teleport(plr.Position);
        ctx.Messages.ReplySuccess("anvil.essentials.teleport.success", plr.Name);
    }

    [Command(["tphere", "teleporthere"], "anvil.essentials.teleporthere")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<player>")]
    [CommandSyntax("ru-RU", "<игрок>")]
    [CommandPermission("anvil.essentials.teleporthere")]
    public static void TeleportHereCommand(PlayerUser user, CommandInvokeContext ctx, PlayerEntity plr)
    {
        plr.Teleport(user.Player.Position);
        ctx.Messages.ReplySuccess("anvil.essentials.teleporthere.success", plr.Name);
    }

    [Command(["tppos", "teleportplayer"], "anvil.essentials.moveplayer")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<x>", "<y>", "[player]")]
    [CommandSyntax("ru-RU", "<x>", "<y>", "[игрок]")]
    [CommandPermission("anvil.essentials.moveplayer")]
    public static void MovePlayerCommand(IAmethystUser user, CommandInvokeContext ctx, int x, int y, PlayerEntity? plr = null)
    {
        if (user is not PlayerUser && plr == null)
        {
            ctx.Messages.ReplyError("anvil.essentials.teleport.notPlayer");
            return;
        }

        plr ??= ((PlayerUser)user).Player;

        if (x < 0 || y < 0 || x >= Main.maxTilesX || y >= Main.maxTilesY)
        {
            ctx.Messages.ReplyError("anvil.essentials.moveplayer.invalidCoordinates", x, y);
            return;
        }

        plr.Teleport(x * 16, y * 16);
        ctx.Messages.ReplySuccess("anvil.essentials.moveplayer.success", plr.Name, x, y);
    }

    [Command("tpspawn", "anvil.essentials.teleportToSpawn")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.essentials.teleportToSpawn")]
    [CommandSyntax("en-US", "<player>")]
    [CommandSyntax("ru-RU", "<игрок>")]
    public static void TeleportToSpawnCommand(IAmethystUser user, CommandInvokeContext ctx, PlayerEntity plr)
    {
        plr.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);
        ctx.Messages.ReplySuccess("anvil.essentials.teleportToSpawn.success", plr.Name);
    }

    [Command("mwarp add", "anvil.essentials.addWarp")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<warp name>", "[x]", "[y]")]
    [CommandSyntax("ru-RU", "<название варпа>", "[x]", "[y]")]
    [CommandPermission("anvil.essentials.warps.manage.add")]
    public static void AddWarpCommand(PlayerUser user, CommandInvokeContext ctx, string warpName)
    {
        if (string.IsNullOrEmpty(warpName))
        {
            ctx.Messages.ReplyError("anvil.essentials.addWarp.invalidName");
            return;
        }

        WarpModel warp = new(warpName)
        {
            Position = user.Player.Position
        };
        warp.Save();
        EssentialsPlugin.ReloadWarps();

        ctx.Messages.ReplySuccess("anvil.essentials.addWarp.success", warpName);
    }

    [Command("mwarp remove", "anvil.essentials.removeWarp")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<warp name>")]
    [CommandSyntax("ru-RU", "<название варпа>")]
    [CommandPermission("anvil.essentials.warps.manage.remove")]
    public static void RemoveWarpCommand(PlayerUser user, CommandInvokeContext ctx, string warpName)
    {
        if (string.IsNullOrEmpty(warpName))
        {
            ctx.Messages.ReplyError("anvil.essentials.removeWarp.invalidName");
            return;
        }

        WarpModel? warp = EssentialsPlugin.LoadedWarps.FirstOrDefault(w => w.Name.Equals(warpName, StringComparison.OrdinalIgnoreCase));
        if (warp == null)
        {
            ctx.Messages.ReplyError("anvil.essentials.removeWarp.notFound", warpName);
            return;
        }

        warp.Remove();
        EssentialsPlugin.ReloadWarps();

        ctx.Messages.ReplySuccess("anvil.essentials.removeWarp.success", warpName);
    }

    [Command("warps", "anvil.essentials.listWarps")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.essentials.warps.use.listWarps")]
    [CommandSyntax("en-US", "[page]")]
    [CommandSyntax("ru-RU", "[страница]")]
    public static void ListWarpsCommand(PlayerUser user, CommandInvokeContext ctx, int page = 1)
    {
        if (EssentialsPlugin.LoadedWarps.Count == 0)
        {
            ctx.Messages.ReplyError("anvil.essentials.listWarps.noWarps");
            return;
        }

        List<string> warpList = EssentialsPlugin.LoadedWarps
            .Select(warp => $"{warp.Name} - {warp.Position.X}, {warp.Position.Y}")
            .ToList();

        PagesCollection pages = PagesCollection.AsListPage(warpList, page);
        ctx.Messages.ReplySuccess("anvil.essentials.listWarps.success", pages);
    }
    [Command("warp", "anvil.essentials.teleportToWarp")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<warp name>")]
    [CommandSyntax("ru-RU", "<название варпа>")]
    [CommandPermission("anvil.essentials.warps.use.teleportToWarp")]
    public static void TeleportToWarpCommand(PlayerUser user, CommandInvokeContext ctx, string warpName)
    {
        if (string.IsNullOrEmpty(warpName))
        {
            ctx.Messages.ReplyError("anvil.essentials.teleportToWarp.invalidName");
            return;
        }

        WarpModel? warp = EssentialsPlugin.LoadedWarps.FirstOrDefault(w => w.Name.Equals(warpName, StringComparison.OrdinalIgnoreCase));
        if (warp == null)
        {
            ctx.Messages.ReplyError("anvil.essentials.teleportToWarp.notFound", warpName);
            return;
        }

        user.Player.Teleport(warp.Position.X, warp.Position.Y);
        ctx.Messages.ReplySuccess("anvil.essentials.teleportToWarp.success", warp.Name);
    }

    [Command("find item", "anvil.essentials.findItem")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<item name ...>")]
    [CommandSyntax("ru-RU", "<название предмета ...>")]
    [CommandPermission("anvil.essentials.findItem")]
    public static void FindItemCommand(PlayerUser user, CommandInvokeContext ctx, string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
        {
            ctx.Messages.ReplyError("anvil.essentials.findItem.invalidName");
            return;
        }

        List<ItemFindData> foundItems = Items.FindItem(false, itemName);
        if (foundItems.Count == 0)
        {
            foundItems.AddRange(Items.FindItem(true, itemName));
        }

        if (foundItems.Count == 0)
        {
            ctx.Messages.ReplyError("anvil.essentials.findItem.notFound", itemName);
            return;
        }

        if (foundItems.Count > 100)
        {
            ctx.Messages.ReplyError("anvil.essentials.findItem.tooManyFound", itemName);
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(foundItems.Select(item => $"{item.Name} ({item.ItemID})").ToList());
        ctx.Messages.ReplyPage(pages, "anvil.essentials.foundItems", null, null, false, 0);
    }

    [Command(["fill", "more all"], "anvil.essentials.fillInventory")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.essentials.fillInventory")]
    public static void FillInventoryCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        if (user.Player == null)
        {
            ctx.Messages.ReplyError("anvil.essentials.fillInventory.notPlayer");
            return;
        }

        if (user.Player.TPlayer.inventory.Any(p => p.type == ItemID.EncumberingStone))
        {
            ctx.Messages.ReplyError("anvil.essentials.fillInventory.encumberingStone");
            return;
        }

        for (int i = 0; i < user.Player.TPlayer.inventory.Length; i++)
        {
            Item item = user.Player.TPlayer.inventory[i];
            if (item.stack < item.maxStack && item.type > 0)
            {
                int amountToAdd = item.maxStack - item.stack;
                if (amountToAdd > 0)
                {
                    user.Player.GiveItem(item.type, amountToAdd, item.prefix);
                }
            }
        }

        ctx.Messages.ReplySuccess("anvil.essentials.fillInventory.success");
    }

    [Command(["fillother"], "anvil.essentials.fillOtherInventory")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<player>")]
    [CommandSyntax("ru-RU", "<игрок>")]
    [CommandPermission("anvil.essentials.fillOtherInventory")]
    public static void FillOtherInventoryCommand(IAmethystUser user, CommandInvokeContext ctx, PlayerEntity plr)
    {
        if (plr == null)
        {
            ctx.Messages.ReplyError("anvil.essentials.fillOtherInventory.notPlayer");
            return;
        }

        if (plr.TPlayer.inventory.Any(p => p.type == ItemID.EncumberingStone))
        {
            ctx.Messages.ReplyError("anvil.essentials.fillOtherInventory.encumberingStone");
            return;
        }

        for (int i = 0; i < plr.TPlayer.inventory.Length; i++)
        {
            Item item = plr.TPlayer.inventory[i];
            if (item.stack < item.maxStack && item.type > 0)
            {
                int amountToAdd = item.maxStack - item.stack;
                if (amountToAdd > 0)
                {
                    plr.GiveItem(item.type, amountToAdd, item.prefix);
                }
            }
        }

        ctx.Messages.ReplySuccess("anvil.essentials.fillOtherInventory.success", plr.Name);
    }
}   