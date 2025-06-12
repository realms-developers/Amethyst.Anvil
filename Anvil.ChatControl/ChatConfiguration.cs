using System.Security.Policy;
using Amethyst.Network.Structures;
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

    public string PlayerJoinedFormat { get; set; } = "{players}/{maxplayers} {name} has joined.";
    public NetColor PlayerJoinedColor { get; set; } = new NetColor(0, 255, 0);

    public string PlayerLeftFormat { get; set; } = "{players}/{maxplayers} {name} has left.";
    public NetColor PlayerLeftColor { get; set; } = new NetColor(255, 0, 0);
}