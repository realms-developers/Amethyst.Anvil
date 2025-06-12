using Amethyst.Kernel;
using Amethyst.Server.Entities;
using Amethyst.Systems.Chat.Misc.Base;
using Amethyst.Systems.Chat.Misc.Context;

namespace Anvil.ChatControl.Misc;

public sealed class PlayerLeftRenderer : IMiscMessageRenderer<PlayerLeftMessageContext>
{
    public string Name => "anvil.chat.playerleft.renderer";

    public MiscRenderedMessage<PlayerLeftMessageContext>? Render(PlayerLeftMessageContext ctx)
    {
        return new MiscRenderedMessage<PlayerLeftMessageContext>(
            ChatConfiguration.Instance.PlayerLeftFormat
                .Replace("{players}", (EntityTrackers.Players.Count() - 1).ToString())
                .Replace("{maxplayers}", AmethystSession.Profile.MaxPlayers.ToString())
                .Replace("{name}", ctx.Player.Name),
            ChatConfiguration.Instance.PlayerLeftColor, ctx);
    }
}