using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.STD;
using System.Runtime.InteropServices;

namespace TeleportAethernet.Game;

[Agent(AgentId.TelepotTown)]
[StructLayout(LayoutKind.Explicit, Size = 0x33)]
public unsafe partial struct AgentTelepotTown
{
    [FieldOffset(0x0)] public AgentInterface AgentInterface;

    // Current selected is at offset `0x07A0`.
    [FieldOffset(0x28)] public StdVector<AethernetInfo> AethernetList;

    [FieldOffset(0x32)] public byte CurrentID;

    [FieldOffset(0x33)] public byte Count;
}

[StructLayout(LayoutKind.Explicit, Size = 0x70A)]
public struct AethernetInfo
{
    [FieldOffset(0x0)]
    public ushort ZoneId;

    [FieldOffset(0x3)]
    public byte HaveIt1;

    [FieldOffset(0x5)]
    public byte HaveIt2;

    [FieldOffset(0x8)]
    public ushort AEKey;

    [FieldOffset(0x10)]
    public ushort PlaceName1;

    [FieldOffset(0x12)]
    public ushort PlaceName2;

    // Seems to work, but I'm pretty sure this value isn't in this struct
    // judging by the large offset.
    //
    // To find the offset:
    // 1. Attach Cheat Engine to the game
    // 2. Open the Aethernet window
    // 3. Select an Aethernet by clicking on it once, so it's highlighted.
    // 4. Search for byte value for the index of the Aethernet in the list.
    // 5. Repeat with other Aethernets until you have one result.
    // 6. Right click the address and select "Find out what writes to this
    //    address"
    // 7. Find a `mov` instruction with a register+offset. The offset is the
    //    value you want.
    //
    // If that doesn't work, maybe the address changed. You can resolve the
    // address by viewing the value of the register (not the offset) in the
    // "Find out what writes to this address" window. You will probably need
    // to search memory for the value of the register to see what points to
    // it, and compare it to values from Dalamud's `/xldata ai` window or
    // Simple Tweak's debug window (Ctrl+Shift + open Simple Tweak settings).
    [FieldOffset(0x70A)]
    public byte CurrentSelected;
}
