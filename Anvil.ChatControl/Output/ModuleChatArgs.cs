using Amethyst.Systems.Chat.Base.Models;

namespace Anvil.ChatControl.Output;

public sealed class ModuleChatArgs
{
    public ModuleChatArgs(string message, MessageRenderResult renderContext)
    {
        RenderedMessage = message;
        RenderContext = renderContext;
    }

    public string RenderedMessage { get; set; }
    public MessageRenderResult RenderContext { get; set; }
}