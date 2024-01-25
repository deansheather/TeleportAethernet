using System.Collections.Generic;

namespace TeleportAethernet.Data;

// TODO: this file shouldn't use any hardcoded strings and should be based on
// the locale. We'll have to find where the game stores the aethernet data in
// memory to accomplish this for the aethernet names, but aetheryte and town
// names should be doable

public static class TownAethernets
{
    public static readonly (uint, byte) FirmamentIDs = (70, 100);

    public static readonly TownAethernet Gridania = new(2, "Gridania", new()
    {
        new(1, 2, "Archers' Guild"),
        new(2, 2, "Leatherworkers' Guild & Shaded Bower"),
        new(3, 2, "Lancers' Guild"),
        new(4, 2, "Conjurers' Guild"),
        new(5, 2, "Botanists' Guild"),
        new(6, 2, "Mih Khetto's Amphitheatre"),
        // These work, but they're probably never used.
        // new(_, 2, "Blue Badger Gate (Central Shroud)"),
        // new(_, 2, "Yellow Serpent Gate (North Shroud)"),
        // new(_, 2, "White Wolf Gate (Central Shroud)"),
        // new(_, 2, "Airship Landing"),
    });

    public static readonly TownAethernet LimsaLominsa = new(8, "Limsa Lominsa", new()
    {
        new(3, 8, "Arcanists' Guild"),
        new(4, 8, "Fishermen's Guild"),
        new(6, 8, "Hawkers' Alley"),
        new(1, 8, "The Aftcastle"),
        new(2, 8, "Culinarians' Guild"),
        new(5, 8, "Marauders' Guild"),
        // These work, but they're probably never used.
        // new(_, 8, "Zephyr Gate (Middle La Noscea)"),
        // new(_, 8, "Tempest Gate (Lower La Noscea)"),
        // new(_, 8, "Airship Landing"),
    });

    public static readonly TownAethernet Uldah = new(9, "Ul'dah", new()
    {
        new(1, 9, "Adventurers' Guild"),
        new(2, 9, "Thaumaturges' Guild"),
        new(3, 9, "Gladiators' Guild"),
        new(4, 9, "Miners' Guild"),
        new(6, 9, "Weavers' Guild"),
        new(7, 9, "Goldsmiths' Guild"),
        new(9, 9, "Sapphire Avenue Exchange"),
        new(5, 9, "Alchemists' Guild"),
        new(8, 9, "The Chamber of Rule"),
        // These work, but they're probably never used.
        // new(_, 9, "Gate of the Sultana (Western Thanalan)"),
        // new(_, 9, "Gate of Nald (Central Thanalan)"),
        // new(_, 9, "Gate of Thal (Central Thanalan)"),
        // new(_, 9, "Airship Landing"),
    });

    public static readonly TownAethernet GoldSaucer = new(62, "The Gold Saucer", new()
    {
        new(1, 62, "Entrance & Card Squares"),
        new(2, 62, "Wonder Square East"),
        new(3, 62, "Wonder Square West"),
        new(4, 62, "Event Square"),
        new(5, 62, "Cactbot Board"),
        new(6, 62, "Round Square"),
        new(7, 62, "Chocobo Square"),
        new(8, 62, "Minion Square"),
    });

    public static readonly TownAethernet Ishgard = new(70, "Ishgard", new()
    {
        new(1, 70, "The Forgotten Knight"),
        new(2, 70, "Skysteel Manufactory"),
        new(3, 70, "The Brume"),
        new(4, 70, "Athenaeum Astrologicum"),
        new(5, 70, "The Jeweled Crozier"),
        new(6, 70, "Saint Reymanaud's Cathedral"),
        new(7, 70, "The Tribunal"),
        new(8, 70, "The Last Vigil"),
        // These work, but they're probably never used.
        // new(_, 70, "The Gates of Judgement (Coerthas Central Highlands)"),

        // Firmament is a special case. It doesn't have an Aethernet Shard, but
        // you can click "Travel to the Firmament" on the Aetheryte menu to
        // teleport to it. TeleportStateMachine has special handling for this
        // specific fake Aethernet ID.
        new(FirmamentIDs.Item2, 70, "The Firmament"),
    });

    public static readonly TownAethernet Idyllshire = new(75, "Idyllshire", new()
    {
        new(1, 75, "West Idyllshire"),
        // These work, but they're probably never used.
        // new(_, 75, "Prologue Gate (Western Hinterlands)"),
        // new(_, 75, "Epilogue Gate (Eastern Hinterlands)"),
    });

    public static readonly TownAethernet RhalgrsReach = new(104, "Rhalgr's Reach", new()
    {
        new(1, 104, "Western Rhalgr's Reach"),
        new(2, 104, "Northeastern Rhalgr's Reach"),
        // These work, but they're probably never used.
        // new(_, 104, "Fringes Gate"),
        // new(_, 104, "Peaks Gate"),
    });

    public static readonly TownAethernet Kugane = new(111, "Kugane", new()
    {
        new(1, 111, "Shiokaze Hostelry"),
        new(2, 111, "Pier #1"),
        new(3, 111, "Thavnairian Consulate"),
        new(4, 111, "Kogane Dori Markets"),
        new(5, 111, "Bokairo Inn"),
        new(6, 111, "The Ruby Bazaar"),
        new(7, 111, "Sekiseigumi Barracks"),
        new(8, 111, "Rakuza District"),
        // These work, but they're probably never used.
        // new(_, 111, "The Ruby Price"),
        // new(_, 111, "Airship Landing"),
    });

    public static readonly TownAethernet DomanEnclave = new(127, "The Doman Enclave", new()
    {
        new(1, 127, "The Northern Enclave"),
        new(2, 127, "The Southern Enclave"),
        new(3, 127, "Ferry Docks"),
        // These work, but they're probably never used.
        // new(_, 127, "The One River"),
    });

    public static readonly TownAethernet Crystarium = new(133, "The Crystarium", new()
    {
        new(1, 133, "Musica Universalis Markets"),
        new(2, 133, "Temenos Rookery"),
        new(3, 133, "The Dossal Gate"),
        new(4, 133, "The Pendants"),
        new(5, 133, "The Amaro Launch"),
        new(6, 133, "The Crystalline Mean"),
        new(7, 133, "The Cabinet of Curiosity"),
        // These work, but they're probably never used.
        // new(_, 133, "Tessellation (Lakeland)"),
    });

    public static readonly TownAethernet Eulmore = new(134, "Eulmore", new()
    {
        new(1, 134, "Southeast Derelicts"),
        new(3, 134, "Nightsoil Pots"),
        new(4, 134, "The Glory Gate"),
        new(2, 134, "The Mainstay"),
        // These work, but they're probably never used.
        // new(_, 134, "The Path to Glory (Kholusia)"),
    });

    public static readonly TownAethernet OldSharlayan = new(182, "Sharlayan", new()
    {
        new(1, 182, "The Studium"),
        new(2, 182, "The Baldesion Annex"),
        new(3, 182, "The Rostra"),
        new(4, 182, "The Leveilleur Estate"),
        new(5, 182, "Journey's End"),
        new(6, 182, "Scholar's Harbor"),
        // These work, but they're probably never used.
        // new(_, 182, "The Hall of Artifice (Labyrinthos)"),
    });

    public static readonly TownAethernet RadzAtHan = new(183, "Radz-at-Han", new()
    {
        new(1, 183, "Meghaduta"),
        new(2, 183, "Ruveydah Fibers"),
        new(3, 183, "Airship Landing"),
        new(4, 183, "Alzadaal's Peace"),
        new(5, 183, "Hall of the Radiant Host"),
        new(6, 183, "Mehryde's Meyhane"),
        new(7, 183, "Kama"),
        new(8, 183, "The High Crucible of Al-Kimiya"),
        // These work, but they're probably never used.
        // new(_, 183, "The Gate of First Sight (Thavnair"),
    });

    public static readonly List<TownAethernet> All = new()
    {
        Gridania,
        LimsaLominsa,
        Uldah,
        GoldSaucer,
        Ishgard,
        Idyllshire,
        RhalgrsReach,
        Kugane,
        DomanEnclave,
        Crystarium,
        Eulmore,
        OldSharlayan,
        RadzAtHan,
    };

    public static TownAethernet? GetByAetheryteID(uint aetheryteID)
    {
        for (var i = 0; i < All.Count; i++)
        {
            if (All[i].AetheryteID == aetheryteID) return All[i];
        }
        return null;
    }
}

public readonly struct TownAethernet(uint aetheryteID, string townName, List<AethernetShard> aethernetList)
{
    // AetheryteID is the ID of the aetheryte used to enter the town.
    public uint AetheryteID { get; } = aetheryteID;

    // TownName is the friendly name of the town in English.
    public string TownName { get; } = townName;

    // AethernetList is the list of aethernet shards in the town.
    public List<AethernetShard> AethernetList { get; } = aethernetList;
}

public readonly struct AethernetShard(byte index, uint aetheryteID, string name)
{
    // Index is the index that the game uses for teleportation. It's not the
    // same as the index in the list or the index in the game's Aethernet UI.
    public byte Index { get; } = index;

    // AetheryteID is the ID of the town Aetheryte.
    public uint AetheryteID { get; } = aetheryteID;

    // Name is the name of the shard as it appears in the game's Aethernet UI
    // in English.
    public string Name { get; } = name;
}
