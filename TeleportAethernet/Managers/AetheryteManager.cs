using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TeleportAethernet.Game;

namespace TeleportAethernet.Managers;

public class AetheryteManager
{
    internal static List<uint> visibleAetherytes = new();

    internal static DateTime lastUpdated = DateTime.MinValue;

    internal static TimeSpan timeSpan = TimeSpan.FromSeconds(5);

    // Events:
    public delegate void OnListUpdatedDelegate(List<uint> newList);
    public static event OnListUpdatedDelegate? OnListUpdated;

    internal static List<uint> GetList()
    {
        if (DateTime.Now - lastUpdated > timeSpan) Update();
        return visibleAetherytes;
    }

    // TODO: does this actually work? test on a new character
    internal static unsafe void Update()
    {
        if (DalamudServices.ClientState.LocalPlayer == null) return;
        DalamudServices.Log.Verbose("AetheryteManager.Update() called");
        lastUpdated = DateTime.Now;

        try
        {
            var tp = Telepo.Instance();
            if (tp->UpdateAetheryteList() == null) return;

            var ids = new List<uint>();
            for (long i = 0; i < tp->TeleportList.LongCount; i++)
            {
                var aetheryte = tp->TeleportList[i];
                ids.Add(aetheryte.AetheryteId);
            }
            ids.Sort();

            // If the list of visible aetherytes has changed, dispatch an
            // event.
            if (!visibleAetherytes.SequenceEqual(ids))
            {
                DalamudServices.Log.Info("Aetherytes list updated");
                visibleAetherytes = ids;
                OnListUpdated?.Invoke(ids);
            }
        }
        catch (Exception e)
        {
            visibleAetherytes = new List<uint>();
            DalamudServices.Log.Info($"Failed to update aetheryte list, considering all aetherytes visible: {e}");
            return;
        }
    }

    public static bool AetheryteIsVisible(uint aetheryteID)
    {
        var list = GetList();
        return list.Count == 0 || list.Contains(aetheryteID);
    }
}
