using Amethyst.Systems.Users.Base.Messages;
using Amethyst.Text;

namespace Anvil.Regions.Working.Users;

public sealed class ExecutorMessageProvider : IMessageProvider
{
    public string Language { get; set; } = "en-US";

    public void ReplyError(string text, params object[] args)
    {}

    public void ReplyInfo(string text, params object[] args)
    {}

    public void ReplyPage(PagesCollection pages, string? header, string? footer, object[]? footerArgs, bool showPageName, int page = 0)
    {}

    public void ReplySuccess(string text, params object[] args)
    {}

    public void ReplyWarning(string text, params object[] args)
    {}
}
