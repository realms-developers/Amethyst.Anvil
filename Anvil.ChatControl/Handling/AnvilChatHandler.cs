using Amethyst.Systems.Chat.Base;
using Amethyst.Systems.Chat.Base.Models;

namespace Anvil.ChatControl.Handling;

public sealed class AnvilChatHandler : IChatMessageHandler
{
    public string Name => "anvil.chat.handler";

    public void HandleMessage(PlayerMessage msg)
    {
        if (msg.Player.User == null)
            return;

        var ext = msg.Player.User.Extensions.GetExtension("anvil.chat.playerchat") as PlayerChatExtension;
        if (ext == null)
        {
            ext = new PlayerChatExtension();
            ext.Load(msg.Player.User);

            msg.Player.User.Extensions.AddExtension(ext);
        }

        if (!ext.CheckCanSendMessage())
        {
            msg.Player.User.Messages.ReplyError("chat.delay");
            msg.Cancel();
            return;
        }

        foreach (var banword in ChatConfiguration.Instance.BanWords)
        {
            if (msg.Text.Contains(banword, StringComparison.OrdinalIgnoreCase))
            {
                msg.Player.User.Messages.ReplyError("chat.bannedword", banword);
                msg.Cancel();
                return;
            }
        }
    }
}