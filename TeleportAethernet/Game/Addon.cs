using FFXIVClientStructs.FFXIV.Client.UI;

namespace TeleportAethernet.Game;

// Taken from https://github.com/Haselnussbomber/HaselCommon/blob/1004b0732b4de8e510fd5c7693f928bdd1778c30/HaselCommon/Utils/Globals/Addon.cs
public static unsafe class Addon
{
    public static T* GetAddon<T>(string name, int index = 1) where T : unmanaged
    {
        var raptureAtkModule = RaptureAtkModule.Instance();
        var addon = raptureAtkModule->RaptureAtkUnitManager.GetAddonByName(name, index);
        var ready = addon != null && addon->IsReady;
        return ready ? (T*)addon : null;
    }
}
