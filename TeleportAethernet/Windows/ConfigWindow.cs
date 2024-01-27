using System;
using System.Numerics;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Interface.Windowing;
using TeleportAethernet.Services;
using ImGuiNET;
using TeleportAethernet.Game;
using TeleportAethernet.Data;
using System.Collections.Generic;

namespace TeleportAethernet.Windows;

public class ConfigWindow : Window
{
    private static readonly Dictionary<TeleportState, string> TeleportStateTooltips = new()
    {
        { TeleportState.TeleportToAetheryte, "Run the teleport to Aetheryte command." },
        { TeleportState.WaitAetheryteTeleportStart, "Wait for the teleport scene transition to start." },
        { TeleportState.WaitAetheryteTeleportEnd, "Wait for the teleportscene transition to end." },
        { TeleportState.CheckAetheryteRange, "Check after teleporting whether the Aetheryte is in range or whether we should move close." },
        { TeleportState.MoveTowardsAetheryte, "Target the Aetheryte, /lockon, /automove on." },
        { TeleportState.WaitAetheryteInRange, "While moving, check whether the Aetheryte is in range." },
        { TeleportState.InteractWithAetheryte, "Once in range, target and interact with the Aetheryte to open the menu." },
        { TeleportState.AetheryteSelectFirmament, "Click the Firmament button in the Aetheryte menu.(if teleporting to Firmament)." },
        { TeleportState.AetheryteSelectAethernet, "Click the Aethernet button in the Aetheryte menu." },
        { TeleportState.AethernetMenu, "Send the commands and write the memory values to teleport to the specific Aethernet while the Aethernet menu is open." },
    };

    private readonly AliasWindow createAliasWindow;

    public ConfigWindow(AliasWindow createAliasWindow) : base(
        "Teleport to Aethernet Config",
        ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        Size = new Vector2(800, 500);
        SizeCondition = ImGuiCond.Appearing;
        this.createAliasWindow = createAliasWindow;
    }

    public override void Draw()
    {
        try
        {
            DrawInner();
        }
        catch (Exception e)
        {
            DalamudServices.Log.Error(e.ToString());
            DalamudServices.PluginInterface.UiBuilder.AddNotification("[TeleportAethernet] Error in ConfigWindow, see logs", null, NotificationType.Error);
        }
    }

    private void DrawInner()
    {
        ImGui.BeginTabBar("ConfigTabs");
        if (ImGui.BeginTabItem("Aliases"))
        {
            DrawAliasesTab();
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Settings"))
        {
            DrawSettingsTab();
            ImGui.EndTabItem();
        }
    }

    private void DrawAliasesTab()
    {
        if (ImGui.Button("New Alias"))
        {
            createAliasWindow.CreateAlias();
        }

        ImGui.BeginChild("Aliases", new Vector2(0, 0), true);
        ImGui.Columns(4, "Alias"); // alias, town name or aetheryte ID, aethernet shard name or index, delete
        ImGui.Text("Alias");
        ImGui.NextColumn();
        ImGui.Text("Town");
        ImGui.NextColumn();
        ImGui.Text("Aethernet Shard");
        ImGui.NextColumn();
        ImGui.Text("Actions");
        ImGui.NextColumn();
        ImGui.Separator();
        for (var i = 0; i < ConfigurationService.Config.AethernetAliases.Count; i++)
        {
            ImGui.PushID($"##alias{i}");
            var alias = ConfigurationService.Config.AethernetAliases[i];

            // Resolve the target town name and Aethernet Shard name.
            // TODO: this should be provided by the Manager class and cached
            // when we start reading values from the game instead of hardcoding
            // them.
            var targetTownName = $"??? ({alias.AetheryteID})";
            var targetAethernetShardName = $"??? ({alias.AethernetIndex})";
            var targetTownAethernet = TownAethernets.GetByAetheryteID(alias.AetheryteID);
            if (targetTownAethernet != null)
            {
                targetTownName = targetTownAethernet.Value.TownName;
                if (alias.AethernetIndex < targetTownAethernet.Value.AethernetList.Count)
                {
                    var name = targetTownAethernet.Value.AethernetList.Find(shard => shard.Index == alias.AethernetIndex).Name;
                    if (name != null) targetAethernetShardName = name;
                }
            }

            ImGui.Text(alias.Alias ?? "???");
            ImGui.NextColumn();
            ImGui.Text(targetTownName + $" ({alias.AetheryteID})");
            ImGui.NextColumn();
            ImGui.Text(targetAethernetShardName + $" ({alias.AethernetIndex})");
            ImGui.NextColumn();
            var isDeleting = ImGui.Button("Delete");
            if (isDeleting && ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyShift)
            {
                ConfigurationService.Config.AethernetAliases.RemoveAt(i);
                ConfigurationService.Save();
                DalamudServices.PluginInterface.UiBuilder.AddNotification($"[TeleportAethernet] Deleted alias {alias.Alias}", null, NotificationType.Success);
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Hold Ctrl+Shift to delete");
                ImGui.EndTooltip();
            }
            ImGui.NextColumn();

            ImGui.PopID();
        }
        ImGui.EndChild();
    }

    private static void DrawSettingsTab()
    {
        var wotsitIntegrationEnabled = ConfigurationService.Config.WotsitIntegrationEnabled;
        ImGui.Text("Wotsit Integration:");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Requires the Wotsit plugin to be enabled. Enables quick searching and selecting of aliases and known Aethernet destinations.");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        if (ImGui.Checkbox("", ref wotsitIntegrationEnabled))
        {
            ConfigurationService.Config.WotsitIntegrationEnabled = wotsitIntegrationEnabled;
            ConfigurationService.Save();
        }

        ImGui.Text("Teleport State Delays and Timeouts:");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Customize the delays and timeouts for each teleport state. The default values are intentionally a little bit slow to avoid executing many actions quickly and looking suspicious, but you can adjust these values to make certain actions quicker or slower.");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        if (ImGui.Button("Reset to Default") && ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyShift)
        {
            ConfigurationService.Config.customTeleportStateDelay = new();
            ConfigurationService.Config.customTeleportStateTimeout = new();
            ConfigurationService.Save();
            DalamudServices.PluginInterface.UiBuilder.AddNotification("[TeleportAethernet] Reset custom teleport state delays and timeouts", null, NotificationType.Success);
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Hold Ctrl+Shift to reset all values");
            ImGui.EndTooltip();
        }
        ImGui.NextColumn();

        ImGui.BeginChild("TeleportState", new Vector2(0, 0), true);
        ImGui.Columns(3, "TeleportState"); // state, delay, timeout
        ImGui.Text("Teleport State");
        ImGui.NextColumn();
        ImGui.Text("Delay (ms)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Delay before starting to execute this state after the previous state finishes (in milliseconds).");
            ImGui.EndTooltip();
        }
        ImGui.NextColumn();
        ImGui.Text("Timeout (ms)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Timeout while executing this state (in milliseconds). Does not include delay. If a state times out, the teleport is canceled.");
            ImGui.EndTooltip();
        }
        ImGui.NextColumn();
        ImGui.Separator();
        foreach (var state in Enum.GetValues<TeleportState>())
        {
            if (state == TeleportState.Completed) continue;

            ImGui.PushID($"##state{state}");
            ImGui.Text(state.ToString());
            var tooltip = TeleportStateTooltips.GetValueOrDefault(state);
            if (tooltip != null && ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(tooltip);
                ImGui.EndTooltip();
            }
            ImGui.NextColumn();

            var delay = ConfigurationService.Config.customTeleportStateDelay.GetValueOrDefault(state);
            var timeout = ConfigurationService.Config.customTeleportStateTimeout.GetValueOrDefault(state);
            var (defaultDelay, defaultTimeout) = TeleportStateMachine.DefaultTeleportStateDelayTimeout.GetValueOrDefault(state);
            delay = delay == 0 ? defaultDelay : delay;
            timeout = timeout == 0 ? defaultTimeout : timeout;

            if (ImGui.InputInt($"##delay{state}", ref delay))
            {
                if (delay < 0) delay = 0;
                if (delay > 30000) delay = 30000;
                ConfigurationService.Config.customTeleportStateDelay[state] = delay == defaultDelay ? 0 : delay;
                ConfigurationService.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                var delayTooltip = $"0 to use default. Default: {defaultDelay}ms";
                ImGui.Text(delayTooltip);
                ImGui.EndTooltip();
            }
            ImGui.NextColumn();

            if (ImGui.InputInt($"##timeout{state}", ref timeout))
            {
                if (timeout < 0) timeout = 0;
                if (timeout > 30000) timeout = 30000;
                ConfigurationService.Config.customTeleportStateTimeout[state] = timeout == defaultTimeout ? 0 : timeout;
                ConfigurationService.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                var timeoutTooltip = $"0 to use default. Default: {defaultTimeout}ms";
                ImGui.Text(timeoutTooltip);
                ImGui.EndTooltip();
            }
            ImGui.NextColumn();

            ImGui.PopID();
        }
        ImGui.EndChild();
    }
}
