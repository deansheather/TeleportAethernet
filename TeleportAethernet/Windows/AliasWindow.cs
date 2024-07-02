using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Windowing;
using TeleportAethernet.Data;
using TeleportAethernet.Services;
using ImGuiNET;
using TeleportAethernet.Game;
using TeleportAethernet.Managers;

namespace TeleportAethernet.Windows;

public class AliasWindow : Window
{
    public string Alias = "";
    public uint AetheryteID = 0;
    public byte AethernetIndex = 0;

    public AliasWindow() : base(
        $"Teleport to Aethernet - Alias Create",
        ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        Size = new Vector2(500, 350);
        SizeCondition = ImGuiCond.Appearing;
    }

    public void CreateAlias()
    {
        Alias = "";
        // Technically this might show a hidden Aetheryte's details, but it's
        // one of the first Aetherytes in the game so it's unlikely.
        AetheryteID = TownAethernets.All[0].AetheryteID;
        AethernetIndex = TownAethernets.All[0].AethernetList[0].Index;
        IsOpen = true;
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
            DalamudServices.NotificationManager.AddNotification(new Notification
            {
                Content = "Error in AliasWindow, see logs",
                Type = NotificationType.Error,
            });
        }
    }

    public void DrawInner()
    {
        ImGui.Text("New Alias:");

        ImGui.Text("Alias:");
        ImGui.InputText("##Alias", ref Alias, 100);

        var aetheryteList = new List<(string, uint, byte)>();
        foreach (var townAethernet in TownAethernets.All)
        {
            foreach (var aethernet in townAethernet.AethernetList)
            {
                aetheryteList.Add((townAethernet.TownName + " - " + aethernet.Name, townAethernet.AetheryteID, aethernet.Index));
            }
        }

        var selected = aetheryteList.Find(a => a.Item2 == AetheryteID && a.Item3 == AethernetIndex);
        ImGui.Text("Aetheryte:");
        if (ImGui.BeginCombo("##Aetheryte", selected.Item1))
        {
            foreach (var aetheryte in aetheryteList)
            {
                // Hide spoilers.
                if (!AetheryteManager.AetheryteIsVisible(aetheryte.Item2)) continue;

                var isSelected = aetheryte.Item2 == AetheryteID && aetheryte.Item3 == AethernetIndex;
                if (ImGui.Selectable(aetheryte.Item1, isSelected))
                {
                    AetheryteID = aetheryte.Item2;
                    AethernetIndex = aetheryte.Item3;
                }
                if (isSelected) ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }

        // Show AetheryteID and AethernetIndex for debugging.
        ImGui.Text($"AetheryteID: {AetheryteID}");
        ImGui.Text($"AethernetIndex: {AethernetIndex}");

        if (ImGui.Button("Save") && Save()) Close();

        foreach (var problem in Validate())
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), problem);
        }
    }

    public List<string> Validate()
    {
        var problems = new List<string>();
        if (Alias.Trim() == "")
        {
            problems.Add("An alias name is required");
        }
        if (AetheryteID == 0)
        {
            problems.Add("Aetheryte ID must be set");
        }
        if (AethernetIndex <= 0)
        {
            problems.Add("Aethernet Index must be set");
        }

        return problems;
    }

    public bool Save()
    {
        if (Validate().Count > 0) return false;

        var alias = new AethernetAlias(null, Alias.Trim(), AetheryteID, AethernetIndex);
        ConfigurationService.Config.AethernetAliases.Add(alias);
        ConfigurationService.Save();
        return true;
    }

    public void Close()
    {
        IsOpen = false;
    }
}
