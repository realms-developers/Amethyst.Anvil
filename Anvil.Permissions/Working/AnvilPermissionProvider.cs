using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Base.Permissions;

namespace Anvil.Permissions.Working;

public sealed class AnvilPermissionProvider : IPermissionProvider
{
    public AnvilPermissionProvider(IAmethystUser user)
    {
        User = user;
    }

    public IAmethystUser User { get; }

    public PermissionWorker Worker { get; } = new();

    private List<IPermissionProvider> _childProviders = new();
    public IReadOnlyList<IPermissionProvider> ChildProviders => _childProviders;

    public bool SupportsChildProviders => true;

    public bool HasChild<T>() where T : IPermissionProvider
    {
        return _childProviders.Any(p => p is T);
    }

    public void AddChild(IPermissionProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        if (_childProviders.Contains(provider))
            return;

        _childProviders.Add(provider);
    }

    public void RemoveChild(IPermissionProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        if (!_childProviders.Contains(provider))
            return;

        _childProviders.Remove(provider);
    }

    public void RemoveChild<T>() where T : IPermissionProvider
    {
        var provider = _childProviders.FirstOrDefault(p => p is T);
        if (provider != null)
        {
            _childProviders.Remove(provider);
        }
    }

    public PermissionAccess HasPermission(string permission)
    {
        throw new NotImplementedException();
    }

    public PermissionAccess HasPermission(PermissionAccess type, int x, int y)
    {
        throw new NotImplementedException();
    }

    public PermissionAccess HasPermission(PermissionAccess type, int x, int y, int width, int height)
    {
        throw new NotImplementedException();
    }
}
