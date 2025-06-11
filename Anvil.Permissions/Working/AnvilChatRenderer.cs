using Amethyst.Server.Systems.Chat.Base;
using Amethyst.Systems.Chat.Base.Models;

namespace Anvil.Permissions.Working;

public sealed class AnvilChatRenderer : IChatMessageRenderer
{
    public string Name => "anvil.perms.renderer";

    public void Render(MessageRenderContext ctx)
    {
        if (ctx.Player.User?.Permissions is not AnvilPermissionProvider provider)
            return;

        if (provider.Worker.RoleModel == null) return;

        var model = provider.Worker.RoleModel;

        if (model.Prefix != null)
            ctx.Prefix.Add("anvil.perms.prefix", model.Prefix);

        if (model.Suffix != null)
            ctx.Suffix.Add("anvil.perms.suffix", model.Suffix);

        if (model.Color.HasValue)
            ctx.Color = model.Color.Value;
    }
}