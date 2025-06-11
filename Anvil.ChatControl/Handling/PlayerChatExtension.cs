using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Base.Extensions;

namespace Anvil.ChatControl.Handling;

public sealed class PlayerChatExtension : IUserExtension
{
    public string Name => "anvil.chat.playerchat";

    public DateTime? LastMessageTime { get; set; } = null;

    public bool CheckCanSendMessage()
    {
        if (LastMessageTime == null)
        {
            LastMessageTime = DateTime.UtcNow;
            return true;
        }

        var delay = ChatConfiguration.Instance.ChatDelay;
        if ((DateTime.UtcNow - LastMessageTime.Value).TotalMilliseconds >= delay)
        {
            LastMessageTime = DateTime.UtcNow;
            return true;
        }

        return false;
    }

    public void Load(IAmethystUser user)
    { }

    public void Unload(IAmethystUser user)
    { }
}
