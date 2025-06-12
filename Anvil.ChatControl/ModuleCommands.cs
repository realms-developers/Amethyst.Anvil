using Amethyst.Systems.Commands.Base;
using Amethyst.Systems.Commands.Dynamic.Attributes;
using Amethyst.Systems.Users.Base;
using Amethyst.Text;

namespace Anvil.ChatControl;

public static class ModuleCommands
{
    [Command(["mchat setformat"], "amethyst.desc.mchat.setformat")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.chat.setformat")]
    [CommandSyntax("en-US", "<format ...>")]
    [CommandSyntax("ru-RU", "<формат ...>")]
    public static void SetChatFormat(IAmethystUser user, CommandInvokeContext ctx)
    {
        string format = string.Join(" ", ctx.Args);

        ChatConfiguration.Instance.OutputFormat = format;
        ChatConfiguration.Configuration.Save();

        user.Messages.ReplySuccess("amethyst.reply.mchat.setformat", format);
    }

    [Command(["mchat getformat"], "amethyst.desc.mchat.getformat")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.chat.getformat")]
    public static void GetChatFormat(IAmethystUser user, CommandInvokeContext ctx)
    {
        string format = ChatConfiguration.Instance.OutputFormat;

        user.Messages.ReplySuccess("amethyst.reply.mchat.getformat", format);
    }

    [Command(["mchat setdelay"], "amethyst.desc.mchat.setdelay")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.chat.setdelay")]
    [CommandSyntax("en-US", "<delay>")]
    [CommandSyntax("ru-RU", "<задержка>")]
    public static void SetChatDelay(IAmethystUser user, CommandInvokeContext ctx, int delay)
    {
        ChatConfiguration.Instance.ChatDelay = delay;
        ChatConfiguration.Configuration.Save();

        user.Messages.ReplySuccess("amethyst.reply.mchat.setdelay", delay);
    }

    [Command(["mchat getdelay"], "amethyst.desc.mchat.getdelay")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.chat.getdelay")]
    public static void GetChatDelay(IAmethystUser user, CommandInvokeContext ctx)
    {
        int delay = ChatConfiguration.Instance.ChatDelay;

        user.Messages.ReplySuccess("amethyst.reply.mchat.getdelay", delay);
    }

    [Command(["mchat addbanword"], "amethyst.desc.mchat.addbanword")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.chat.addbanword")]
    [CommandSyntax("en-US", "<word ...>")]
    [CommandSyntax("ru-RU", "<слово ...>")]
    public static void AddBanWord(IAmethystUser user, CommandInvokeContext ctx)
    {
        string word = string.Join(" ", ctx.Args);

        if (word.Trim().Length == 0)
        {
            user.Messages.ReplyError("amethyst.reply.mchat.addbanword.empty");
            return;
        }

        if (ChatConfiguration.Instance.BanWords.Contains(word, StringComparer.OrdinalIgnoreCase))
        {
            user.Messages.ReplyError("amethyst.reply.mchat.banword.exists", word);
            return;
        }

        ChatConfiguration.Instance.BanWords.Add(word);
        ChatConfiguration.Configuration.Save();

        user.Messages.ReplySuccess("amethyst.reply.mchat.addbanword", word);
    }

    [Command(["mchat removebanword"], "amethyst.desc.mchat.removebanword")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.chat.removebanword")]
    [CommandSyntax("en-US", "<word ...>")]
    [CommandSyntax("ru-RU", "<слово ...>")]
    public static void RemoveBanWord(IAmethystUser user, CommandInvokeContext ctx)
    {
        string word = string.Join(" ", ctx.Args);

        if (word.Trim().Length == 0)
        {
            user.Messages.ReplyError("amethyst.reply.mchat.removebanword.empty");
            return;
        }

        if (!ChatConfiguration.Instance.BanWords.Remove(word))
        {
            user.Messages.ReplyError("amethyst.reply.mchat.banword.notexists", word);
            return;
        }

        ChatConfiguration.Configuration.Save();

        user.Messages.ReplySuccess("amethyst.reply.mchat.removebanword", word);
    }

    [Command(["mchat listbanwords"], "amethyst.desc.mchat.listbanwords")]
    [CommandRepository("shared")]
    [CommandPermission("anvil.chat.listbanwords")]
    [CommandSyntax("en-US", "[page]")]
    [CommandSyntax("ru-RU", "[страница]")]
    public static void ListBanWords(IAmethystUser user, CommandInvokeContext ctx, int page = 0)
    {
        var pages = PagesCollection.AsListPage(ChatConfiguration.Instance.BanWords);
        user.Messages.ReplyPage(pages, "amethyst.reply.mchat.listbanwords", null, null, false, page);
    }
}