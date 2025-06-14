using Amethyst.Extensions.Base.Metadata;
using Amethyst.Extensions.Modules;
using Amethyst.Hooks;
using Amethyst.Hooks.Args.Players;
using Amethyst.Hooks.Context;
using Amethyst.Kernel;
using Amethyst.Network.Handling.Base;
using Anvil.Audit;
using Anvil.Regions.Data;
using Anvil.Regions.Data.Models;
using Anvil.Regions.Network;
using Anvil.Regions.Working.Permissions;

namespace Anvil.Regions;

[ExtensionMetadata("Anvil.Regions", "realms-developers")]
public static class RegionsModule
{
    public static AuditInstance AuditInstance { get; } = AuditModule.GetInstance("Anvil.Permissions");
    public static IReadOnlyList<RegionModel> Regions => _regions.AsReadOnly();
    private static List<RegionModel> _regions = new();

    private static bool _initialized;

    [ModuleInitialize]
    public static void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        ReloadRegions();

        HookRegistry.GetHook<PlayerPostSetUserArgs>().Register(static (in PlayerPostSetUserArgs args, HookResult<PlayerPostSetUserArgs> result) =>
        {
            if (args.User == null)
            {
                return;
            }

            args.User.Permissions.AddChild(new RegionPermissionProvider(args.User));

            args.User.Extensions.AddExtension(new Working.Extensions.PlayerRegionExtension());
            args.User.Extensions.GetExtension("anvil.region")!.Load(args.User);
        });

        HandlerManager.RegisterHandler(new ProtectionHandler());
        HandlerManager.RegisterHandler(new MarkingHandler());
    }

    public static void ReloadRegions()
    {
        _regions = ModuleStorage.Regions.FindAll(p => p.ServerName == null || p.ServerName == AmethystSession.Profile.Name).OrderBy(p => p.Z).ToList();
    }
}