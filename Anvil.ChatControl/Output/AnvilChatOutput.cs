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
        bool GetValueOrDefault(string key, out string? outValue)
        {
            if (message.Prefix.TryGetValue(key, out var value) ||
                message.Name.TryGetValue(key, out value) ||
                message.Suffix.TryGetValue(key, out value) ||
                message.Text.TryGetValue(key, out value))
            {
                outValue = value;
                return true;
            }

            outValue = null;
            return false;
        }

        List<string> configParts = ParseConfig();
        string output = ChatConfiguration.Instance.OutputFormat;

        foreach (var part in configParts)
        {
            if (!part.StartsWith("{") || !part.EndsWith("}"))
            {
                continue;
            }

            bool canContinue = false;
            var key = part.Trim('{', '}');
            if (key.Contains('|'))
            {
                foreach (var subpart in key.Split('|'))
                {
                    if (GetValueOrDefault(subpart, out var value1))
                    {
                        output = output.Replace(part, value1);

                        canContinue = true;
                        break;
                    }
                }

                if (canContinue)
                    continue;
            }


            output = GetValueOrDefault(key, out var value2) ?
                output.Replace(part, value2) :
                output.Replace(part, string.Empty);
        }

        output = output.Trim('{').Trim('}');

        AmethystLog.Main.Info("Chat", output.RemoveColorTags());
        PlayerUtils.BroadcastText(output, message.Color.R, message.Color.G, message.Color.B);

        HookRegistry.GetHook<ModuleChatArgs>().Invoke(new ModuleChatArgs(output, message));
    }

    private List<string> ParseConfig()
    {
        var config = ChatConfiguration.Instance.OutputFormat;
        var parts = new List<string>();
        int startIndex = 0;

        while (startIndex < config.Length)
        {
            int openBraceIndex = config.IndexOf('{', startIndex);
            if (openBraceIndex == -1)
            {
                parts.Add(config.Substring(startIndex));
                break;
            }

            if (openBraceIndex > startIndex)
            {
                parts.Add(config.Substring(startIndex, openBraceIndex - startIndex));
            }

            int closeBraceIndex = config.IndexOf('}', openBraceIndex);
            if (closeBraceIndex == -1)
            {
                parts.Add(config.Substring(openBraceIndex));
                break;
            }

            parts.Add(config.Substring(openBraceIndex, closeBraceIndex - openBraceIndex + 1));
            startIndex = closeBraceIndex + 1;
        }

        return parts;   
    }
}