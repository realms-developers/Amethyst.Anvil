using Amethyst;
using Amethyst.Hooks;
using Amethyst.Server.Entities.Players;
using Amethyst.Systems.Chat.Base;
using Amethyst.Systems.Chat.Base.Models;
using Amethyst.Text;

namespace Anvil.ChatControl.Output;

public sealed class AnvilChatOutput : IChatMessageOutput
{
    public string Name => "ConfigurableOutput";

    public void OutputMessage(MessageRenderResult message)
    {
        List<string> configParts = ParseConfig();
        string output = ChatConfiguration.Instance.OutputFormat;

        foreach (var part in configParts)
        {
            bool canContinue = false;
            var key = part.Trim('{', '}');
            foreach (var subpart in part.Split('|'))
            {
                if (message.Prefix.TryGetValue(key, out var value) ||
                    message.Name.TryGetValue(key, out value) ||
                    message.Suffix.TryGetValue(key, out value) ||
                    message.Text.TryGetValue(key, out value))
                {
                    output = output.Replace($"{{{key}}}", value);

                    canContinue = true;
                    break;
                }
            }

            if (!canContinue)
            {
                output = output.Replace($"{{{key}}}", string.Empty);
            }
        }

        AmethystLog.Main.Info("Chat", output.RemoveColorTags());
        PlayerUtils.BroadcastText(output, message.Color.R, message.Color.G, message.Color.B);

        HookRegistry.GetHook<ModuleChatArgs>().Invoke(new ModuleChatArgs(output, message));
    }

    private List<string> ParseConfig()
    {
        var config = ChatConfiguration.Instance.OutputFormat;
        var parts = config.Split(' ');
        var result = new List<string>();

        foreach (var part in parts)
        {
            if (part.StartsWith("{") && part.EndsWith("}"))
            {
                // Handle placeholders
                var placeholder = part.Trim('{', '}');
                result.Add($"{{{placeholder}}}");
            }
            else
            {
                // Regular text
                result.Add(part);
            }
        }

        return result;
    }
}