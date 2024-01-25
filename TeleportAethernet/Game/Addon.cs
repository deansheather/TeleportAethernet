using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TeleportAethernet.Game;

// Taken from https://github.com/Haselnussbomber/HaselCommon/blob/ac6b873925459bb5797dcdbdce03d8852619c5cf/HaselCommon/Utils/Globals/Agent.cs
public static unsafe class Agent
{
    private static readonly Dictionary<Type, AgentId> AgentIdCache = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* GetAgent<T>(AgentId id) where T : unmanaged
        => (T*)AgentModule.Instance()->GetAgentByInternalID((uint)id);

    public static T* GetAgent<T>() where T : unmanaged
    {
        var type = typeof(T);

        if (!AgentIdCache.TryGetValue(type, out var id))
        {
            var attr = type.GetCustomAttribute<AgentAttribute>(false)
                ?? throw new Exception($"Agent {type.FullName} is missing AgentAttribute");

            AgentIdCache.Add(type, id = attr.ID);
        }

        return GetAgent<T>(id);
    }
}

// Taken from https://github.com/Haselnussbomber/HaselCommon/blob/ac6b873925459bb5797dcdbdce03d8852619c5cf/HaselCommon/Utils/Globals/Addon.cs
public static unsafe class Addon
{
    public static T* GetAddon<T>(string name, int index = 1) where T : unmanaged
    {
        var raptureAtkModule = RaptureAtkModule.Instance();
        var addon = raptureAtkModule->RaptureAtkUnitManager.GetAddonByName(name, index);
        var ready = addon != null && raptureAtkModule->AtkModule.IsAddonReady(addon->ID);
        return ready ? (T*)addon : null;
    }

    public static T* GetAddon<T>(ushort addonId) where T : unmanaged
    {
        var raptureAtkModule = RaptureAtkModule.Instance();
        var ready = raptureAtkModule != null && raptureAtkModule->AtkModule.IsAddonReady(addonId);
        return ready ? (T*)raptureAtkModule->RaptureAtkUnitManager.GetAddonById(addonId) : null;
    }

    public static T* GetAddon<T>(AgentId agentId) where T : unmanaged
    {
        var agent = Agent.GetAgent<AgentInterface>(agentId);
        var active = agent != null && agent->IsAgentActive();
        return active ? GetAddon<T>((ushort)agent->GetAddonID()) : null;
    }

    // ---

    public static bool TryGetAddon<T>(string name, int index, out T* addon) where T : unmanaged
        => (addon = GetAddon<T>(name, index)) != null;

    public static bool TryGetAddon<T>(string name, out T* addon) where T : unmanaged
        => (addon = GetAddon<T>(name, 1)) != null;

    public static bool TryGetAddon<T>(ushort addonId, out T* addon) where T : unmanaged
        => (addon = GetAddon<T>(addonId)) != null;

    public static bool TryGetAddon<T>(AgentId agentId, out T* addon) where T : unmanaged
        => (addon = GetAddon<T>(agentId)) != null;

    // ---

    public static bool IsAddonOpen(string name, int index = 1)
        => GetAddon<AtkUnitBase>(name, index) != null;

    public static bool IsAddonOpen(ushort addonId)
        => GetAddon<AtkUnitBase>(addonId) != null;

    public static bool IsAddonOpen(AgentId agentId)
        => GetAddon<AtkUnitBase>(agentId) != null;
}
