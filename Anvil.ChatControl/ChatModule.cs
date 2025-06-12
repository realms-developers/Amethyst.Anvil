using Amethyst;
using Amethyst.Extensions.Base.Metadata;
using Amethyst.Extensions.Modules;
using Amethyst.Systems.Chat;

namespace Anvil.ChatControl;

[ExtensionMetadata("Anvil.ChatControl", "realms-developers")]
public static class ChatModule
{
    private static bool _initialized;

    [ModuleInitialize]
    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        var primitiveOutput = ServerChat.OutputRegistry.Handlers.FirstOrDefault(p => p.Name == "PrimitiveOutput");
        if (primitiveOutput != null)
        {
            ServerChat.OutputRegistry.Remove(primitiveOutput);
            AmethystLog.System.Info("ChatControl", "PrimitiveOutput removed from OutputRegistry.");
        }
        else
        {
            AmethystLog.System.Warning("ChatControl", "PrimitiveOutput not found in OutputRegistry, cannot remove it.");
        }

        ServerChat.OutputRegistry.Add(new Output.AnvilChatOutput());
        AmethystLog.System.Info("ChatControl", "AnvilChatOutput added to OutputRegistry.");

        ServerChat.RendererRegistry.Add(new Rendering.AnvilChatRenderer());
        AmethystLog.System.Info("ChatControl", "AnvilChatRenderer added to RendererRegistry.");

        ServerChat.HandlerRegistry.Add(new Handling.AnvilChatHandler());
        AmethystLog.System.Info("ChatControl", "AnvilChatHandler added to HandlerRegistry.");
    }
}
