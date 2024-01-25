using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TeleportAethernet.Game;
using TeleportAethernet.Services;

namespace TeleportAethernet.Managers;

public class AetheryteManager
{
    internal static List<uint> visibleAetherytes = new();
    
    internal static DateTime lastUpdated = DateTime.MinValue;

    internal static TimeSpan timeSpan = TimeSpan.FromSeconds(5);

    internal static List<uint> GetList()
    {
        if (DateTime.Now - lastUpdated > timeSpan) Update();
        return visibleAetherytes;
    }

    internal static unsafe void Update()
    {
        if (DalamudServices.ClientState.LocalPlayer == null) return;

        try
        {
            var tp = Telepo.Instance();
            if (tp->UpdateAetheryteList() == null) return;

            var ids = new List<uint>();
            for (ulong i = 0; i < tp->TeleportList.Size(); i++)
            {
                var aetheryte = tp->TeleportList.Get(i);
                ids.Add(aetheryte.AetheryteId);
            }
            ids.Sort();

            // If the list of visible aetherytes has changed, trigger a Wotsit
            // update.
            if (!visibleAetherytes.SequenceEqual(ids))
            {
                DalamudServices.Log.Info("Aetherytes list updated");
                visibleAetherytes = ids;
                ConfigurationService.WotsitManager.TryInit();
            }
            lastUpdated = DateTime.Now;
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
        return GetList().Contains(aetheryteID);
    }
}
