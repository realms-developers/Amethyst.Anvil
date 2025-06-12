using Amethyst.Storages.Config;

namespace Anvil.ChatControl;

public sealed class ChatConfiguration
{
    static ChatConfiguration()
    {
        Configuration = new($"Anvil.ChatOuput", new());
        Configuration.Load();
    }

    public static Configuration<ChatConfiguration> Configuration { get; }
    public static ChatConfiguration Instance => Configuration.Data;

    public string OutputFormat { get; set; } = "{anvil.chat.customprefix|anvil.perms.prefix} {realname} [{anvil.chat.life}/{anvil.chat.maxlife}] {anvil.perms.suffix}: {modifiedtext|realtext}";

    public int ChatDelay { get; set; } = 1000;
    public bool EnableChatDelay { get; set; } = true;

    public List<string> BanWords { get; set; } = [];
}