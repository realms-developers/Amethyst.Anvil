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
        if (User.Suspensions?.IsSuspended == true)
        {
            return ChildHandlePermission(p => p.HasPermission(permission));
        }

        var result = Worker.HasPermission(permission);
        return result == PermissionAccess.None ? ChildHandlePermission(p => p.HasPermission(permission)) : result;
    }

    public PermissionAccess HasPermission(PermissionType type, int x, int y)
    {
        return HandleWorldPermission(type) ?? ChildHandlePermission(p => p.HasPermission(type, x, y));
    }

    public PermissionAccess HasPermission(PermissionType type, int x, int y, int width, int height)
    {
        return HandleWorldPermission(type) ?? ChildHandlePermission(p => p.HasPermission(type, x, y));
    }


    private PermissionAccess ChildHandlePermission(Func<IPermissionProvider, PermissionAccess> action)
    {
        bool hasPermission = false;

        foreach (var provider in _childProviders)
        {
            var result = action(provider);
            if (result == PermissionAccess.HasPermission)
            {
                hasPermission = true;
                break;
            }
            else if (result == PermissionAccess.Blocked)
            {
                return PermissionAccess.Blocked;
            }
        }

        return hasPermission ? PermissionAccess.HasPermission : PermissionAccess.None;
    }

    private PermissionAccess? HandleWorldPermission(PermissionType type)
    {
        if (User.Suspensions?.IsSuspended == true)
        {
            Console.WriteLine($"Suspended => Blocked");
            return PermissionAccess.Blocked;
        }

        Console.WriteLine($"Not suspended => {HasPermission("world." + type.ToString().ToLowerInvariant()) != PermissionAccess.HasPermission}");
        return HasPermission("world." + type.ToString().ToLowerInvariant()) != PermissionAccess.HasPermission ? PermissionAccess.Blocked : null;;
    }
}
