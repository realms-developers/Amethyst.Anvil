using Amethyst;
using Amethyst.Network.Structures;
using Amethyst.Systems.Commands.Base;
using Amethyst.Systems.Commands.Dynamic.Attributes;
using Amethyst.Systems.Users.Players;
using Amethyst.Text;
using Terraria;
using Terraria.Localization;
using static Amethyst.Localization;

namespace Anvil.Essentials;

public static class PluginCommands
{
    [Command(["i", "item"], "Gives the specified item to the player.")]
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
}