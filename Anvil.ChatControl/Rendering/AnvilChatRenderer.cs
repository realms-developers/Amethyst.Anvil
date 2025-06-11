using Amethyst.Server.Systems.Chat.Base;
using Amethyst.Systems.Chat.Base.Models;

namespace Anvil.ChatControl.Rendering;

public sealed class AnvilChatRenderer : IChatMessageRenderer
{
    public string Name => "anvil.chat.renderer";

    public void Render(MessageRenderContext ctx)
    {
        ctx.Name.TryAdd("realname", ctx.Player.Name);

        ctx.Prefix.Add("anvil.chat.life", ctx.Player.Life.ToString());
        ctx.Prefix.Add("anvil.chat.maxlife", ctx.Player.MaxLife.ToString());
        ctx.Prefix.Add("anvil.chat.mana", ctx.Player.Mana.ToString());
        ctx.Prefix.Add("anvil.chat.maxmana", ctx.Player.MaxMana.ToString());
    }
}