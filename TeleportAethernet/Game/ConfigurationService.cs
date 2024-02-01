using TeleportAethernet.Data;
using TeleportAethernet.Game;

namespace TeleportAethernet.Services;

internal class ConfigurationService
{
    private static Config config { get; set; } = null!;

    // Events:
    public delegate void OnConfigSavedDelegate();
    public static event OnConfigSavedDelegate? OnConfigSaved;

    internal static Config Config
    {
        get
        {
            if (config == null) Load();
            return config!;
        }
    }

    internal static void Load()
    {
        config = DalamudServices.PluginInterface.GetPluginConfig() as Config ?? new Config();
    }

    internal static void Save()
    {
        DalamudServices.PluginInterface.SavePluginConfig(Config);
        OnConfigSaved?.Invoke();
    }
}
