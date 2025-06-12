using Amethyst.Kernel;
using Amethyst.Server.Entities;
using Amethyst.Systems.Chat.Misc.Base;
using Amethyst.Systems.Chat.Misc.Context;

namespace Anvil.ChatControl.Misc;

public sealed class PlayerJoinedRenderer : IMiscMessageRenderer<PlayerJoinedMessageContext>
{
    public string Name => "anvil.chat.playerjoined.renderer";

    public MiscRenderedMessage<PlayerJoinedMessageContext>? Render(PlayerJoinedMessageContext ctx)
    {
        return new MiscRenderedMessage<PlayerJoinedMessageContext>(
            ChatConfiguration.Instance.PlayerJoinedFormat
                .Replace("{players}", EntityTrackers.Players.Count().ToString())
                .Replace("{maxplayers}", AmethystSession.Profile.MaxPlayers.ToString())
                .Replace("{name}", ctx.Player.Name),

            ChatConfiguration.Instance.PlayerJoinedColor, ctx);
    }
}
