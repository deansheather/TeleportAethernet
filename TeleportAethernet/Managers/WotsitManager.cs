using Dalamud.Plugin.Ipc;
using System;
using System.Collections.Generic;
using TeleportAethernet.Data;
using TeleportAethernet.Game;
using TeleportAethernet.Services;

namespace TeleportAethernet.Managers;

public class WotsitManager : IDisposable
{
    private static readonly string PluginInternalName = "Teleport to Aethernet";

    // Map from Guid string to (aetheryte ID, aethernet index).
    private readonly Dictionary<string, (uint, byte)> registered = new();

    // Dalamud IPC:
    private readonly ICallGateSubscriber<bool> faAvailable;
    private readonly ICallGateSubscriber<string, bool> faInvoke;
    private ICallGateSubscriber<string, string, string, uint, string>? faRegisterWithSearch;

    // Events:
    public delegate void OnInvokeDelegate(uint aetheryteID, byte aethernetIndex);
    public event OnInvokeDelegate? OnInvoke;

    public WotsitManager()
    {
        faAvailable = DalamudServices.PluginInterface.GetIpcSubscriber<bool>("FA.Available");
        faAvailable.Subscribe(TryInit);
        faInvoke = DalamudServices.PluginInterface.GetIpcSubscriber<string, bool>("FA.Invoke");
        faInvoke.Subscribe(HandleInvoke);
    }

    public void Dispose()
    {
        ClearWotsit();
        faAvailable?.Unsubscribe(TryInit);
        faInvoke?.Unsubscribe(HandleInvoke);
        GC.SuppressFinalize(this);
    }

    private void HandleInvoke(string id)
    {
        if (!registered.ContainsKey(id)) return;
        OnInvoke?.Invoke(registered[id].Item1, registered[id].Item2);
    }

    public void ClearWotsit()
    {
        var faUnregisterAll = DalamudServices.PluginInterface.GetIpcSubscriber<string, bool>("FA.UnregisterAll");
        faUnregisterAll!.InvokeFunc(PluginInternalName);
        DalamudServices.Log.Debug($"WotsitManager: Invoked FA.UnregisterAll(\"{PluginInternalName}\")");
        registered.Clear();
    }

    public void TryInit()
    {
        TryInit(AetheryteManager.GetList());
    }

    public void TryInit(List<uint> visibleAetheryteIDs)
    {
        try
        {
            Init(visibleAetheryteIDs);
        }
        catch (Exception e)
        {
            DalamudServices.Log.Warning($"Failed to initialize WotsitManager. {e}");
        }
    }

    private bool ShouldAddEntry(List<uint> visibleAetheryteIDs, uint aetheryteID)
    {
        // If there are no visible Aetherytes, there might've been an issue
        // loading the list.
        return visibleAetheryteIDs.Count == 0 || visibleAetheryteIDs.Contains(aetheryteID);
    }

    public void Init(List<uint> visibleAetheryteIDs)
    {
        ClearWotsit();

        var config = ConfigurationService.Config;
        if (!config.WotsitIntegrationEnabled) return;

        faRegisterWithSearch = DalamudServices.PluginInterface.GetIpcSubscriber<string, string, string, uint, string>("FA.RegisterWithSearch");

        foreach (var alias in config.AethernetAliases)
        {
            if (!ShouldAddEntry(visibleAetheryteIDs, alias.AetheryteID)) continue;
            AddWotsitEntry(null, alias.Alias, alias.AetheryteID, alias.AethernetIndex);
        }

        foreach (var townAethernet in TownAethernets.All)
        {
            foreach (var aethernet in townAethernet.AethernetList)
            {
                if (!ShouldAddEntry(visibleAetheryteIDs, townAethernet.AetheryteID)) continue;
                AddWotsitEntry(townAethernet.TownName, aethernet.Name, townAethernet.AetheryteID, aethernet.Index);
            }
        }
    }

    internal void AddWotsitEntry(string? townName, string name, uint aetheryteID, byte aethernetIndex)
    {
        var displayName = $"Teleport to Aethernet - {name}";
        var searchStr = townName != null ? $"{townName} - {name}" : name;

        // TODO: icon ID
        var id = faRegisterWithSearch!.InvokeFunc(PluginInternalName, displayName, searchStr, 0);
        registered.Add(id, (aetheryteID, aethernetIndex));
        DalamudServices.Log.Debug($"WotsitManager: Invoked FA.RegisterWithSearch(\"{PluginInternalName}\", \"{displayName}\", \"{name}\", 0)");
        DalamudServices.Log.Debug($"WotsitManager: Added Wotsit mapping: {id} => ({aetheryteID}, {aethernetIndex})");
    }
}
