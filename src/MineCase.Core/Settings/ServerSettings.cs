using System.ComponentModel;
using Newtonsoft.Json;
using Orleans;

namespace MineCase.Server.Settings
{
    [Orleans.GenerateSerializer]
    [JsonObject(MemberSerialization.OptOut)]
    public class ServerSettings
    {
        // server
        [JsonProperty(PropertyName = "motd")]
        [DefaultValue("A Minecraft Server")]
        [Id(0)]
        public string Motd { get; set; } // this is the message that is displayed in the server list of the client

        [JsonProperty(PropertyName = "server-ip")]
        [DefaultValue("127.0.0.1")]
        [Id(1)]
        public string ServerIp { get; set; }

        [JsonProperty(PropertyName = "server-port")]
        [DefaultValue(25565)]
        [Id(2)]
        public uint ServerPort { get; set; }

        [JsonProperty(PropertyName = "max-tick-time")]
        [DefaultValue(60000)]
        [Id(3)]
        public uint MaxTickTime { get; set; }

        [JsonProperty(PropertyName = "online-mode")]
        [DefaultValue(false)]
        [Id(4)]
        public bool OnlineMode { get; set; }

        [JsonProperty(PropertyName = "enable-rcon")]
        [DefaultValue(false)]
        [Id(5)]
        public bool EnableRcon { get; set; } // Enables remote access to the server console.

        /// <summary>
        /// Gets or sets by default it allows packets that are n-1 bytes big to go normally, but a packet that n bytes or more will be compressed down. So, lower number means more compression but compressing small amounts of bytes might actually end up with a larger result than what went in.
        /// -1 - disable compression entirely
        /// 0 - compress everything.
        /// </summary>
        [JsonProperty(PropertyName = "network-compression-threshold")]
        [DefaultValue(256)]
        [Id(6)]
        public uint NetworkCompressionThreshold { get; set; }

        [JsonProperty(PropertyName = "resource-pack")]
        [DefaultValue("")]
        [Id(7)]
        public string ResourcePack { get; set; }

        [JsonProperty(PropertyName = "resource-pack-hash")]
        [DefaultValue("")]
        [Id(8)]
        public string ResourcePackHash { get; set; }

        [JsonProperty(PropertyName = "snooper-enabled")]
        [DefaultValue(true)]
        [Id(9)]
        public bool SnooperEnabled { get; set; }

        [JsonProperty(PropertyName = "enable-query")]
        [DefaultValue(false)]
        [Id(10)]
        public bool EnableQuery { get; set; }

        // spawn
        [JsonProperty(PropertyName = "spawn-protection")]
        [DefaultValue(16)]
        [Id(11)]
        public uint SpawnProtection { get; set; }

        [JsonProperty(PropertyName = "spawn-monsters")]
        [DefaultValue(true)]
        [Id(12)]
        public bool SpawnMonsters { get; set; }

        [JsonProperty(PropertyName = "spawn-npcs")]
        [DefaultValue(true)]
        [Id(13)]
        public bool SpawnNpcs { get; set; }

        [JsonProperty(PropertyName = "spawn-animals")]
        [DefaultValue(true)]
        [Id(14)]
        public bool SpawnAnimals { get; set; }

        // world
        [JsonProperty(PropertyName = "max-world-size")]
        [DefaultValue(29999984)]
        [Id(15)]
        public uint MaxWorldSize { get; set; }

        [JsonProperty(PropertyName = "allow-nether")]
        [DefaultValue(true)]
        [Id(16)]
        public bool AllowNether { get; set; }

        [JsonProperty(PropertyName = "generate-structures")]
        [DefaultValue(true)]
        [Id(17)]
        public bool GenerateStructures { get; set; }

        [JsonProperty(PropertyName = "generate-settings")]
        [DefaultValue("")]
        [Id(18)]
        public string GenerateSettings { get; set; }

        [JsonProperty(PropertyName = "max-build-height")]
        [DefaultValue(256)]
        [Id(19)]
        public uint MaxBuildHeight { get; set; }

        [JsonProperty(PropertyName = "level-seed")]
        [DefaultValue("")]
        [Id(20)]
        public string LevelSeed { get; set; } // seed for you world

        [JsonProperty(PropertyName = "level-type")]
        [DefaultValue("DEFAULT")]
        [Id(21)]
        public string LevelType { get; set; } // type of you world

        [JsonProperty(PropertyName = "enable-command-block")]
        [DefaultValue(false)]
        [Id(22)]
        public bool EnableCommandBlock { get; set; } // type of you world

        [JsonProperty(PropertyName = "hardcore")]
        [DefaultValue(false)]
        [Id(23)]
        public bool Hardcore { get; set; } // If set to true, players will be set to spectator mode if they die.

        // user
        [JsonProperty(PropertyName = "max-players")]
        [DefaultValue(100)]
        [Id(24)]
        public uint MaxPlayers { get; set; }

        [JsonProperty(PropertyName = "view-distance")]
        [DefaultValue(10)]
        [Id(25)]
        public uint ViewDistance { get; set; }

        [JsonProperty(PropertyName = "allow-flight")]
        [DefaultValue(false)]
        [Id(26)]
        public bool AllowFlight { get; set; }

        [JsonProperty(PropertyName = "player-idle-timeout")]
        [DefaultValue(0)]
        [Id(27)]
        public uint PlayerIdleTimeout { get; set; } // If non-zero, players are kicked from the server if they are idle for more than that many minutes.

        [JsonProperty(PropertyName = "difficulty")]
        [DefaultValue(1)]
        [Id(28)]
        public uint Difficulty { get; set; }

        [JsonProperty(PropertyName = "force-gamemode")]
        [DefaultValue(false)]
        [Id(29)]
        public bool ForceGamemode { get; set; }

        [JsonProperty(PropertyName = "gamemode")]
        [DefaultValue(0)]
        [Id(30)]
        public uint Gamemode { get; set; }

        [JsonProperty(PropertyName = "pvp")]
        [DefaultValue(true)]
        [Id(31)]
        public bool Pvp { get; set; }

        [JsonProperty(PropertyName = "announce-player-achievements")]
        [DefaultValue(true)]
        [Id(32)]
        public bool AnnouncePlayerAchievements { get; set; }

        // op
        [JsonProperty(PropertyName = "white-list")]
        [DefaultValue(false)]
        [Id(33)]
        public bool WhiteList { get; set; }

        [JsonProperty(PropertyName = "op-permission-level")]
        [DefaultValue(4)]
        [Id(34)]
        public uint OpPermissionLevel { get; set; }
    }
}
