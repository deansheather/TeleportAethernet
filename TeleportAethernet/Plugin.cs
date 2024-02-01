using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using TeleportAethernet.Game;
using System;
using Dalamud.Plugin.Services;
using System.Linq;
using TeleportAethernet.Data;
using Dalamud.Interface.Windowing;
using TeleportAethernet.Windows;
using TeleportAethernet.Services;
using TeleportAethernet.Managers;

namespace TeleportAethernet;

public sealed class Plugin : IDalamudPlugin
{
    internal static string Name => "Teleport to Aethernet";

    private const string CommandName = "/teleportaethernet";

    private readonly WindowSystem windowSystem = new();

    private readonly ConfigWindow configWindow;

    private readonly AliasWindow aliasWindow;

    private readonly WotsitManager wotsitManager;

    private TeleportStateMachine? teleportStateMachine;

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface
    )
    {
        pluginInterface.Create<DalamudServices>();

        DalamudServices.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Teleport to the given aethernet name"
        });

        DalamudServices.CommandManager.AddHandler("/tpa", new CommandInfo(OnCommand)
        {
            HelpMessage = "Alias for " + CommandName
        });


        aliasWindow = new AliasWindow();
        windowSystem.AddWindow(aliasWindow);
        configWindow = new ConfigWindow(aliasWindow);
        windowSystem.AddWindow(configWindow);

        // Initialize Aetherytes list and initialize Wotsit integration.
        var aetheryteList = AetheryteManager.GetList();
        wotsitManager = new WotsitManager();
        wotsitManager.TryInit(aetheryteList);
        AetheryteManager.OnListUpdated += wotsitManager.TryInit;
        ConfigurationService.OnConfigSaved += wotsitManager.TryInit;
        wotsitManager.OnInvoke += SetTeleport;

        DalamudServices.Framework.Update += OnFrameworkUpdate;
        DalamudServices.PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        DalamudServices.PluginInterface.UiBuilder.OpenConfigUi += ShowConfigWindow;
    }

    public void Dispose()
    {
        DalamudServices.CommandManager.RemoveHandler(CommandName);
        DalamudServices.Framework.Update -= OnFrameworkUpdate;
        DalamudServices.PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        DalamudServices.PluginInterface.UiBuilder.OpenConfigUi -= ShowConfigWindow;

        AetheryteManager.OnListUpdated -= wotsitManager.TryInit;
        ConfigurationService.OnConfigSaved -= wotsitManager.TryInit;
        wotsitManager.OnInvoke -= SetTeleport;

        wotsitManager.Dispose();
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (teleportStateMachine != null)
        {
            try
            {
                if (!teleportStateMachine.OnFrameworkUpdate())
                {
                    teleportStateMachine = null;
                }
            }
            catch (Exception e)
            {
                teleportStateMachine = null;
                DalamudServices.Log.Warning($"Exception in TeleportStateMachine: {e}");
            }
        }
    }

    private unsafe void OnCommand(string command, string args)
    {
        try
        {
            if (args == "config")
            {
                configWindow.IsOpen = true;
                return;
            }

            if (args.StartsWith("wotsit"))
            {
                if (args == "wotsit clear")
                {
                    try
                    {
                        wotsitManager.ClearWotsit();
                    }
                    catch (Exception e)
                    {
                        DalamudServices.Log.Warning($"Clear Wotsit integration on user request: {e}");
                        DalamudServices.ChatGui.PrintError($"Failed to clear Wotsit: {e}");
                        return;
                    }
                    DalamudServices.ChatGui.Print("Cleared Wotsit integration items.");
                }
                else if (args == "wotsit init")
                {
                    try
                    {
                        wotsitManager.Init(AetheryteManager.GetList());
                    }
                    catch (Exception e)
                    {
                        DalamudServices.Log.Warning($"Init Wotsit integration on user request: {e}");
                        DalamudServices.ChatGui.PrintError($"Failed to init Wotsit: {e}");
                        return;
                    }
                    DalamudServices.ChatGui.Print("Reinitalized Wotsit integration.");
                }
                else
                {
                    DalamudServices.ChatGui.PrintError("Unknown Wotsit command.");
                }
                return;
            }

            if (teleportStateMachine != null)
            {
                // Already teleporting.
                return;
            }

            uint aetheryteID = 0;
            byte aethernetIndex = 0;

            // Parse args for debug command.
            var split = args.Split(' ');
            if (split.Length == 3 && split[0] == "debug")
            {
                aetheryteID = uint.Parse(split[1]);
                aethernetIndex = byte.Parse(split[2]);

                if (aetheryteID == 0)
                {
                    // Instead of teleporting, just click the Aethernet menu.
                    TeleportStateMachine.SendAethernetMenuEvent(aethernetIndex);
                    return;
                }
            }

            // Otherwise, use the whole arg string as the name/alias.
            var aethernetName = args.ToLower();
            if (aethernetName == "")
            {
                DalamudServices.ChatGui.PrintError("Aethernet name or alias required.");
                return;
            }

            // Try to find an alias.
            var alias = ConfigurationService.Config.AethernetAliases.FirstOrDefault(a => a.Alias.ToLower() == aethernetName);
            if (alias != null)
            {
                aetheryteID = alias.AetheryteID;
                aethernetIndex = alias.AethernetIndex;
            }
            else
            {
                // Otherwise, just find one matching the full name.
                try
                {
                    var shard = TownAethernets.All
                        .SelectMany(town => town.AethernetList)
                        .First(shard => shard.Name.ToLower() == aethernetName);
                    aetheryteID = shard.AetheryteID;
                    aethernetIndex = shard.Index;
                }
                catch (InvalidOperationException)
                {
                    DalamudServices.ChatGui.PrintError($"Could not find Aethernet node or alias named '{aethernetName}'.");
                    return;
                }
            }

            if (aetheryteID == 0 || aethernetIndex == 0)
            {
                DalamudServices.ChatGui.PrintError($"Could not find Aethernet node or alias named '{aethernetName}'.");
                return;
            }

            SetTeleport(aetheryteID, aethernetIndex);
        }
        catch (Exception e)
        {
            DalamudServices.Log.Warning($"Exception in OnCommand: {e}");
            DalamudServices.ChatGui.PrintError($"Exception in OnCommand: {e}");
            return;
        }
    }

    public void SetTeleport(uint aetheryteID, byte aethernetIndex)
    {
        if (teleportStateMachine != null) return;
        teleportStateMachine = new TeleportStateMachine(aetheryteID, aethernetIndex);
    }

    public void ShowConfigWindow()
    {
        configWindow.IsOpen = true;
    }
}
