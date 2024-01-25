using Dalamud.Plugin.Ipc;
using System;
using System.Collections.Generic;
using TeleportAethernet.Data;
using TeleportAethernet.Game;
using TeleportAethernet.Services;

namespace TeleportAethernet.Managers;

public class WotsitManager : IDisposable
{
    private static readonly string PluginInternalName = "TeleportAethernet";

    // Map from Guid string to (aetheryte ID, aethernet index).
    private readonly Dictionary<string, (uint, byte)> registered = new();

    private readonly ICallGateSubscriber<bool> faAvailable;
    private readonly ICallGateSubscriber<string, bool> faInvoke;
    private ICallGateSubscriber<string, bool>? faUnregisterAll;
    private ICallGateSubscriber<string, string, string, uint, string>? faRegisterWithSearch;

    // Events:
    public delegate void OnInvokeDelegate(uint aetheryteID, byte aethernetIndex);
    public event OnInvokeDelegate? OnInvoke;

    public WotsitManager()
    {
        try
        {
            TryInit();
        }
        catch (Exception e)
        {
            DalamudServices.Log.Warning($"Failed to initialize WotsitManager. {e}");
        }

        faAvailable = DalamudServices.PluginInterface.GetIpcSubscriber<bool>("FA.Available");
        faAvailable.Subscribe(TryInit);
        faInvoke = DalamudServices.PluginInterface.GetIpcSubscriber<string, bool>("FA.Invoke");
        faInvoke.Subscribe(HandleInvoke);
    }

    public void Dispose()
    {
        ClearWotsit();
        faAvailable.Unsubscribe(TryInit);
        faInvoke.Unsubscribe(HandleInvoke);
        GC.SuppressFinalize(this);
    }

    private void HandleInvoke(string id)
    {
        if (!registered.ContainsKey(id)) return;
        OnInvoke?.Invoke(registered[id].Item1, registered[id].Item2);
    }

    private void ClearWotsit()
    {
        // TODO: this doesn't seem to work :/
        faUnregisterAll = DalamudServices.PluginInterface.GetIpcSubscriber<string, bool>("FA.UnregisterAll");
        faUnregisterAll!.InvokeFunc(PluginInternalName);
        registered.Clear();
    }

    public void TryInit()
    {
        ClearWotsit();

        var config = ConfigurationService.Config;
        if (!config.WotsitIntegrationEnabled) return;

        faRegisterWithSearch = DalamudServices.PluginInterface.GetIpcSubscriber<string, string, string, uint, string>("FA.RegisterWithSearch");

        foreach (var alias in config.AethernetAliases)
        {
            AddWotsitEntry(alias.Alias, alias.AetheryteID, alias.AethernetIndex);
        }

        foreach (var townAethernet in TownAethernets.All)
        {
            foreach (var aethernet in townAethernet.AethernetList)
            {
                AddWotsitEntry(aethernet.Name, townAethernet.AetheryteID, aethernet.Index);
            }
        }
    }

    internal void AddWotsitEntry(string name, uint aetheryteID, byte aethernetIndex)
    {
        // Hide spoilers.
        if (!AetheryteManager.AetheryteIsVisible(aetheryteID)) return;

        var displayName = $"Teleport to Aethernet - {name}";

        // TODO: icon ID
        var id = faRegisterWithSearch!.InvokeFunc(PluginInternalName, displayName, name, 0);
        registered.Add(id, (aetheryteID, aethernetIndex));
    }
}
