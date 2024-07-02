using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Linq;
using TeleportAethernet.Game;
using TeleportAethernet.Data;
using System.Collections.Generic;
using ECommons;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using TeleportAethernet.Services;

namespace TeleportAethernet;

public enum TeleportState
{
    TeleportToAetheryte,        // => WaitAetheryteTeleportStart
    WaitAetheryteTeleportStart, // => WaitAetheryteTeleportEnd
    WaitAetheryteTeleportEnd,   // => CheckAetheryteRange
    CheckAetheryteRange,        // => MoveTowardsAetheryte, InteractWithAetheryte
    MoveTowardsAetheryte,       // => WaitAetheryteInRange
    WaitAetheryteInRange,       // => InteractWithAetheryte
    InteractWithAetheryte,      // => AetheryteSelectFirmament, AetheryteSelectAethernet
    AetheryteSelectFirmament,   // => Completed
    AetheryteSelectAethernet,   // => AethernetMenu
    AethernetMenu,              // => Completed
    Completed,
}

internal class TeleportStateMachine
{
    // The actual value is 11.0f, but sometimes it stops too early somehow.
    // Maybe the hitbox is irregular or something?
    // If we're stuck on a wall or something and can't reach 10.0f, we'll stop
    // auto movement after 1s and try to interact anyways.
    public static readonly float MAX_AETHERYTE_INTERACT_DISTANCE = 10.0f;

    // Delay and timeout values in milliseconds for each TeleportStateMachine
    // state.
    public static readonly Dictionary<TeleportState, (int, int)> DefaultTeleportStateDelayTimeout = new()
    {
        { TeleportState.TeleportToAetheryte, (10, 3000) },
        { TeleportState.WaitAetheryteTeleportStart, (0, 10000) }, // this includes cast time
        { TeleportState.WaitAetheryteTeleportEnd, (100, 20000) }, // this includes loading time
        { TeleportState.CheckAetheryteRange, (1500, 8000) },      // may include a little bit of loading time
        { TeleportState.MoveTowardsAetheryte, (0, 3000) },
        { TeleportState.WaitAetheryteInRange, (100, 10000) },     // this includes walking time
        { TeleportState.InteractWithAetheryte, (500, 10000) },
        { TeleportState.AetheryteSelectFirmament, (1000, 5000) },
        { TeleportState.AetheryteSelectAethernet, (1000, 5000) },
        { TeleportState.AethernetMenu, (500, 5000) },
        { TeleportState.Completed, (0, 0) },
    };

    // Params:
    public uint AetheryteID { get; init; }

    public byte AethernetIndex { get; init; }

    public uint TerritoryID { get; init; }

    // State:
    public TeleportState State { get; private set; }

    private DateTime LastStateTransition { get; set; } = DateTime.Now;

    private DateTime? Timeout { get; set; } = null;

    private DateTime? Delay { get; set; } = null;

    public TeleportStateMachine(uint aetheryteID, byte aethernetIndex)
    {
        AetheryteID = aetheryteID;
        AethernetIndex = aethernetIndex;

        // Get the territory ID associated with this Aetheryte.
        var aetherytes = DalamudServices.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets2.Aetheryte>() ?? throw new Exception("Lumina.Excel.GeneratedSheets2.Aetheryte sheet is null");
        var aetheryte = aetherytes.FirstOrDefault(a => a.RowId == AetheryteID) ?? throw new Exception($"Aetheryte with ID {AetheryteID} not found");
        TerritoryID = aetheryte.Territory.Row;
        if (TerritoryID == 0) throw new Exception("Could not determine aetheryte territory ID");

        DalamudServices.Log.Info($"TeleportStateMachine: constructed AetheryteId={AetheryteID} AethernetID={AethernetIndex} TerritoryID={TerritoryID}");
        // TODO: print to chat "Teleporting to AethernetName in TerritoryName via AetheryteName"
        SetState(TeleportState.TeleportToAetheryte);
    }

    // OnFrameworkUpdate returns true if the state machine is still
    // running, or false if it has completed.
    public bool OnFrameworkUpdate()
    {
        if (Timeout != null && DateTime.Now > Timeout)
        {
            HandleTimeout();
            return false;
        }

        if (Delay != null && DateTime.Now < Delay) return true;

        switch (State)
        {
            case TeleportState.TeleportToAetheryte:
                TeleportToAetheryte();
                break;
            case TeleportState.WaitAetheryteTeleportStart:
                WaitAetheryteTeleportStart();
                break;
            case TeleportState.WaitAetheryteTeleportEnd:
                WaitAetheryteTeleportEnd();
                break;
            case TeleportState.CheckAetheryteRange:
                CheckAetheryteRange();
                break;
            case TeleportState.MoveTowardsAetheryte:
                MoveTowardsAetheryte();
                break;
            case TeleportState.WaitAetheryteInRange:
                WaitAetheryteInRange();
                break;
            case TeleportState.InteractWithAetheryte:
                InteractWithAetheryte();
                break;
            case TeleportState.AetheryteSelectFirmament:
                AetheryteSelectFirmament();
                break;
            case TeleportState.AetheryteSelectAethernet:
                AetheryteSelectAethernet();
                break;
            case TeleportState.AethernetMenu:
                AethernetMenu();
                break;
            case TeleportState.Completed:
                return false;
        }

        return true;
    }

    private void SetState(TeleportState state)
    {
        State = state;
        LastStateTransition = DateTime.Now;

        var (delayMilliseconds, timeoutMilliseconds) = DefaultTeleportStateDelayTimeout.GetValueOrDefault(State);

        // Try load user custom values.
        var customDelay = ConfigurationService.Config.customTeleportStateDelay.GetValueOrDefault(State);
        delayMilliseconds = customDelay > 0 ? customDelay : delayMilliseconds;
        var customTimeout = ConfigurationService.Config.customTeleportStateTimeout.GetValueOrDefault(State);
        timeoutMilliseconds = customTimeout > 0 ? customTimeout : timeoutMilliseconds;

        Delay = delayMilliseconds != 0 ? LastStateTransition.AddMilliseconds(delayMilliseconds) : null;
        Timeout = timeoutMilliseconds != 0 ? LastStateTransition.AddMilliseconds(delayMilliseconds + timeoutMilliseconds) : null;

        DalamudServices.Log.Info($"TeleportStateMachine: SetState({state}), timeoutMS={timeoutMilliseconds}, delayMS={delayMilliseconds}");
    }

    private unsafe void TeleportToAetheryte()
    {
        Telepo.Instance()->Teleport(AetheryteID, 0);
        SetState(TeleportState.WaitAetheryteTeleportStart);
    }

    private static bool BetweenAreas => DalamudServices.Condition.Any(ConditionFlag.BetweenAreas, ConditionFlag.BetweenAreas51);

    private void WaitAetheryteTeleportStart()
    {
        if (!BetweenAreas) return;
        SetState(TeleportState.WaitAetheryteTeleportEnd);
    }

    private void WaitAetheryteTeleportEnd()
    {
        if (BetweenAreas) return;

        // Verify we're in the expected territory.
        var currentTerritory = DalamudServices.ClientState.TerritoryType;
        if (currentTerritory != TerritoryID)
        {
            throw new Exception($"Teleported to unexpected territory, AetheryteID={AetheryteID}, CurrentTerritoryID={currentTerritory}, ExpectedTerritoryID={TerritoryID}");
        }

        SetState(TeleportState.CheckAetheryteRange);
    }

    private static IGameObject NearestAetheryte()
    {
        if (DalamudServices.ClientState.LocalPlayer == null) throw new Exception("DalamudServices.ClientState.LocalPlayer is null");

        var closestDistance = float.MaxValue;
        IGameObject? closestObj = null;
        foreach (var obj in DalamudServices.ObjectTable)
        {
            if (obj is { ObjectKind: ObjectKind.Aetheryte })
            {
                var distance = DistanceToGameObject(obj);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObj = obj;
                }
            }
        }
        if (closestObj != null)
        {
            return closestObj;
        }

        throw new Exception("No Aetheryte object found nearby");
    }

    private static float DistanceToGameObject(IGameObject obj)
    {
        if (DalamudServices.ClientState.LocalPlayer == null) throw new Exception("DalamudServices.ClientState.LocalPlayer is null");

        var x = obj.Position.X - DalamudServices.ClientState.LocalPlayer.Position.X;
        var z = obj.Position.Z - DalamudServices.ClientState.LocalPlayer.Position.Z;
        return (float)Math.Sqrt((x * x) + (z * z));
    }

    private void CheckAetheryteRange()
    {
        if (DalamudServices.ClientState.LocalPlayer == null) return;

        var aetheryteObj = NearestAetheryte();

        // Target it.
        DalamudServices.TargetManager.Target = aetheryteObj;

        // Send `/lockon`. This doesn't need to be undone as it will be undone
        // automatically when the Aetheryte is untargeted.
        ChatService.Instance.SendMessage("/lockon");

        var distance = DistanceToGameObject(aetheryteObj);
        SetState(distance > MAX_AETHERYTE_INTERACT_DISTANCE ? TeleportState.MoveTowardsAetheryte : TeleportState.InteractWithAetheryte);
    }

    private void MoveTowardsAetheryte()
    {
        if (DalamudServices.ClientState.LocalPlayer == null) return;

        // Send `/automove on`. This needs to be undone when we're in range of
        // the Aetheryte.
        ChatService.Instance.SendMessage("/automove on");

        SetState(TeleportState.WaitAetheryteInRange);
    }

    private void WaitAetheryteInRange()
    {
        if (DalamudServices.ClientState.LocalPlayer == null) return;

        var aetheryteObj = NearestAetheryte();

        // Either wait till we're in range, or until 1s has passed.
        var distance = DistanceToGameObject(aetheryteObj);
        var timeSinceStateTransition = DateTime.Now - LastStateTransition;
        var waitTime = TimeSpan.FromSeconds(1);
        if (AetheryteID == TownAethernets.SolutionNine.AetheryteID)
            // Walk a bit longer in Solution Nine.
            waitTime = TimeSpan.FromSeconds(3);
        if (distance < MAX_AETHERYTE_INTERACT_DISTANCE || timeSinceStateTransition > waitTime)
        {
            // Undo `/automove on`.
            ChatService.Instance.SendMessage("/automove off");
            SetState(TeleportState.InteractWithAetheryte);
        }
    }

    private unsafe void InteractWithAetheryte()
    {
        if (DalamudServices.ClientState.LocalPlayer == null) return;

        var aetheryteObj = NearestAetheryte();

        // Interact with it.
        var aetheryteGameObj = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)aetheryteObj.Address;
        TargetSystem.Instance()->InteractWithObject(aetheryteGameObj);

        // Special case for The Firmament in Ishgard.
        if ((AetheryteID, AethernetIndex) == TownAethernets.FirmamentIDs)
        {
            SetState(TeleportState.AetheryteSelectFirmament);
        }
        else
        {
            SetState(TeleportState.AetheryteSelectAethernet);
        }
    }

    private static unsafe bool ClickAetheryteMenu(ushort index)
    {
        // Get AddonSelectString and ensure it's ready.
        var addonSelectString = Addon.GetAddon<AddonSelectString>("SelectString");
        if (addonSelectString == null || !addonSelectString->AtkUnitBase.IsVisible || addonSelectString->AtkUnitBase.UldManager.LoadedState != AtkLoadState.Loaded)
            return false;

        if (new AddonMaster.SelectString((nint)addonSelectString).Entries.TryGetFirst(x => x.Index == index, out var entry))
        {
            entry.Select();
            return true;
        }

        return false;
    }

    private void AetheryteSelectFirmament()
    {
        // Click "Travel to the Firmament.", then we're done.
        // TODO: we should verify whether the quest to unlock the Firmament is
        //       complete, before attempting to teleport there. If the button
        //       doesn't exist, then this script will click "Set Home Point"
        //       instead, which is benign (you get a confirmation dialog).
        if (ClickAetheryteMenu(2)) SetState(TeleportState.Completed);
    }

    private void AetheryteSelectAethernet()
    {
        // Click "Aethernet."
        if (ClickAetheryteMenu(0)) SetState(TeleportState.AethernetMenu);
    }

    // This is public because a debug command uses it.
    public static unsafe bool SendAethernetMenuEvent(byte aethernetIndex)
    {
        var agentTelepotTown = AgentTelepotTown.Instance();
        if (agentTelepotTown == null || !agentTelepotTown->IsAddonReady() || !agentTelepotTown->IsAgentActive())
            return false;

        agentTelepotTown->TeleportToAetheryte(aethernetIndex);
        return true;
    }

    private void AethernetMenu()
    {
        if (SendAethernetMenuEvent(AethernetIndex))
            SetState(TeleportState.Completed);
    }

    private void HandleTimeout()
    {
        DalamudServices.Log.Warning($"TeleportStateMachine: timeout in state {State}");
        DalamudServices.ChatGui.PrintError("Timed out during Aethernet teleport operation. See Dalamud plugin logs for more details.");
        // TODO: print to screen

        // Stop auto movement.
        ChatService.Instance.SendMessage("/automove off");

        SetState(TeleportState.Completed);
    }
}
