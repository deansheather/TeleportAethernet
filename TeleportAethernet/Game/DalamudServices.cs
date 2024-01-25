using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace TeleportAethernet.Game
{
    public class DalamudServices
    {
        [PluginService]
        [RequiredVersion("1.0")]
        public static DalamudPluginInterface PluginInterface { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static IPluginLog Log { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static ICommandManager CommandManager { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static IFramework Framework { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static IObjectTable ObjectTable { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static IClientState ClientState { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static ITargetManager TargetManager { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static IDataManager DataManager { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static ICondition Condition { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static ISigScanner SigScanner { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static IAetheryteList AetheryteList { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static IChatGui ChatGui { get; private set; } = null!;
    }
}

