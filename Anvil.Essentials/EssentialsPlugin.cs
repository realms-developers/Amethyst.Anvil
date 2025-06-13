using Amethyst.Essentials.Data.Models;
using Amethyst.Extensions.Base.Metadata;
using Amethyst.Extensions.Plugins;
using Anvil.Essentials.Data;

namespace Anvil.Essentials;

[ExtensionMetadata("Anvil.Essentials", "realms-developers", "Provides essential features for Amethyst.API Terraria servers.")]
public sealed class EssentialsPlugin : PluginInstance
{
    public static List<WarpModel> LoadedWarps { get; private set; } = [];

    protected override void Load()
    {
        ReloadWarps();
    }

    protected override void Unload()
    {
    }

    public static void ReloadWarps()
    {
        LoadedWarps = PluginStorage.Regions.FindAll().ToList();
    }
}