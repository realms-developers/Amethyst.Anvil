using Amethyst.Systems.Characters.Base;
using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Base.Commands;
using Amethyst.Systems.Users.Base.Extensions;
using Amethyst.Systems.Users.Base.Messages;
using Amethyst.Systems.Users.Base.Permissions;
using Amethyst.Systems.Users.Base.Requests;
using Amethyst.Systems.Users.Base.Suspension;
using Amethyst.Systems.Users.Common.Commands;
using Amethyst.Systems.Users.Common.Requests;

namespace Anvil.Regions.Working.Users;

public sealed class ExecutorUser : IAmethystUser
{
    internal ExecutorUser()
    {
        Commands = new CommonCommandProvider(this, 0, ["shared"]);
        Permissions = new ExecutorPermissionProvider(this);
        Extensions = null!;
    }

    public string Name => "regions_usr";

    public IMessageProvider Messages { get; } = new ExecutorMessageProvider();

    public IPermissionProvider Permissions { get; }

    public IExtensionProvider Extensions { get; }

    public ICommandProvider Commands { get; }

    public ICharacterProvider? Character => null;

    public ISuspensionProvider? Suspensions => null;

    public IRequestProvider Requests { get; } = new CommonRequestProvider();
}
