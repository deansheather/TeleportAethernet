using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace TeleportAethernet.Game
{
    public class DalamudServices
    {
        [PluginService]
        public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

        [PluginService]
        public static IPluginLog Log { get; private set; } = null!;

        [PluginService]
        public static ICommandManager CommandManager { get; private set; } = null!;

        [PluginService]
        public static IFramework Framework { get; private set; } = null!;

        [PluginService]
        public static IObjectTable ObjectTable { get; private set; } = null!;

        [PluginService]
        public static IClientState ClientState { get; private set; } = null!;

        [PluginService]
        public static ITargetManager TargetManager { get; private set; } = null!;

        [PluginService]
        public static IDataManager DataManager { get; private set; } = null!;

        [PluginService]
        public static ICondition Condition { get; private set; } = null!;

        [PluginService]
        public static IChatGui ChatGui { get; private set; } = null!;

        [PluginService]
        public static INotificationManager NotificationManager { get; private set; } = null!;

        [PluginService]
        public static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    }
}

