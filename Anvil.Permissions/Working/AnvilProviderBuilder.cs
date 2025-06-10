using Amethyst.Permissions.Data.User;
using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Base.Permissions;
using Amethyst.Systems.Users.Players;
using Anvil.Permissions.Storage;

namespace Anvil.Permissions.Working;

public sealed class AnvilProviderBuilder : IProviderBuilder<IPermissionProvider>
{
    public IPermissionProvider BuildFor(IAmethystUser user)
    {
        if (user is not PlayerUser)
        {
            throw new ArgumentException("AnvilPermissionProvider can only be built for PlayerUser instances.", nameof(user));
        }

        var provider = new AnvilPermissionProvider(user);

        UserModel? model = ModuleStorage.Users.Find(user.Name);
        if (model == null)
        {
            model = new UserModel(user.Name);
            model.Role = ModuleStorage.Roles.Find(p => p.IsDefault)?.Name;
            model.Save();
        }

        provider.Worker.Assign(model);

        return provider;
    }
}
