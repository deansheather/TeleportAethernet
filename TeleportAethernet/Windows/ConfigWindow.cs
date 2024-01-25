using System;
using System.Numerics;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Interface.Windowing;
using TeleportAethernet.Services;
using ImGuiNET;
using TeleportAethernet.Game;
using TeleportAethernet.Data;

namespace TeleportAethernet.Windows;

public class ConfigWindow : Window
{
    private readonly AliasWindow createAliasWindow;

    public ConfigWindow(AliasWindow createAliasWindow) : base(
        "Teleport to Aethernet Config",
        ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        Size = new Vector2(500, 350);
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

    public void DrawInner()
    {
        var wotsitIntegrationEnabled = ConfigurationService.Config.WotsitIntegrationEnabled;
        ImGui.Text("Wotsit Integration:");
        ImGui.SameLine();
        if (ImGui.Checkbox("", ref wotsitIntegrationEnabled))
        {
            ConfigurationService.Config.WotsitIntegrationEnabled = wotsitIntegrationEnabled;
            ConfigurationService.Save();
        }

        ImGui.Text("Aliases:");
        ImGui.SameLine();
        if (ImGui.Button("New"))
        {
            DalamudServices.Log.Information("Create alias button pressed");
            createAliasWindow.CreateAlias();
        }

        // Scrolling table of aliases.
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
                DalamudServices.Log.Information("Delete button pressed");
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
    }
}
