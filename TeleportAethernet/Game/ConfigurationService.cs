using TeleportAethernet.Data;
using TeleportAethernet.Game;
using TeleportAethernet.Managers;

namespace TeleportAethernet.Services;

internal class ConfigurationService
{
    private static Config config { get; set; } = null!;

    internal static Config Config
    {
        get
        {
            if (config == null) Load();
            return config!;
        }
    }

    private static WotsitManager wotsitManager { get; set; } = null!;

    internal static WotsitManager WotsitManager
    {
        get
        {
            wotsitManager ??= new WotsitManager();
            return wotsitManager;
        }
    }

    internal static void Load() {
        config = DalamudServices.PluginInterface.GetPluginConfig() as Config ?? new Config();
    }

    internal static void Save() {
        DalamudServices.PluginInterface.SavePluginConfig(Config);
        WotsitManager.TryInit();
    }
}
