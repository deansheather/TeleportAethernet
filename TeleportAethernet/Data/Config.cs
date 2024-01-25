using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using TeleportAethernet.Game;

namespace TeleportAethernet.Data
{
    [Serializable]
    public class AethernetAlias(Guid? id, string alias, uint aetheryteID, byte aethernetIndex)
    {
        public Guid ID { get; } = id ?? Guid.NewGuid();

        public string Alias { get; } = alias;

        public uint AetheryteID { get; } = aetheryteID;

        public byte AethernetIndex { get; } = aethernetIndex;
    }

    [Serializable]
    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        // Whether or not we should integrate with Wotsit. All known Aethernet
        // nodes and aliases below get added to Wotsit.
        public bool WotsitIntegrationEnabled { get; set; } = true;

        // Aliases for Aethernets. These work for `/tpa` and also get sent over
        // IPC to the Wotsit plugin.
        public List<AethernetAlias> AethernetAliases { get; set; } = new();
    }
}
