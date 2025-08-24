using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using MineCase.Server.Game;
using Newtonsoft.Json;

namespace MineCase.Server.Settings
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ServerSettings
    {
        // server
        [DefaultValue("A Minecraft Server")]
        public string Motd { get; set; } // this is the message that is displayed in the server list of the client

        [DefaultValue("127.0.0.1")]
        public string ServerIp { get; set; }

        [DefaultValue(25565)]
        public uint ServerPort { get; set; }

        [DefaultValue(60000)]
        public uint MaxTickTime { get; set; }

        [DefaultValue(false)]
        public bool OnlineMode { get; set; }

        [DefaultValue(false)]
        public bool EnableRcon { get; set; } // Enables remote access to the server console.

        /// <summary>
        /// Gets or sets by default it allows packets that are n-1 bytes big to go normally, but a packet that n bytes or more will be compressed down. So, lower number means more compression but compressing small amounts of bytes might actually end up with a larger result than what went in.
        /// -1 - disable compression entirely
        /// 0+ - compress everything bigger than or equal to N.
        /// </summary>
        [DefaultValue(256)]
        public uint NetworkCompressionThreshold { get; set; }

        [DefaultValue("")]
        public string ResourcePack { get; set; }

        [DefaultValue("")]
        public string ResourcePackHash { get; set; }

        [DefaultValue(true)]
        public bool SnooperEnabled { get; set; }

        [DefaultValue(false)]
        public bool EnableQuery { get; set; }

        // spawn
        [DefaultValue(16)]
        public uint SpawnProtection { get; set; }

        [DefaultValue(true)]
        public bool SpawnMonsters { get; set; }

        [DefaultValue(true)]
        public bool SpawnNpcs { get; set; }

        [DefaultValue(true)]
        public bool SpawnAnimals { get; set; }

        // world
        [DefaultValue(29999984)]
        public uint MaxWorldSize { get; set; }

        [DefaultValue(true)]
        public bool AllowNether { get; set; }

        [DefaultValue(true)]
        public bool GenerateStructures { get; set; }

        [DefaultValue("")]
        public string GeneratorSettings { get; set; }

        [DefaultValue(256)]
        public uint MaxBuildHeight { get; set; }

        [DefaultValue("")]
        public string LevelSeed { get; set; } // seed for you world

        [DefaultValue("DEFAULT")]
        public string LevelType { get; set; } // type of you world

        [DefaultValue(false)]
        public bool EnableCommandBlock { get; set; } // type of you world

        [DefaultValue(false)]
        public bool Hardcore { get; set; } // If set to true, players will be set to spectator mode if they die.

        // user
        [DefaultValue(100)]
        public uint MaxPlayers { get; set; }

        [DefaultValue(10)]
        public uint ViewDistance { get; set; }

        [DefaultValue(false)]
        public bool AllowFlight { get; set; }

        [DefaultValue(0)]
        public uint PlayerIdleTimeout { get; set; } // If non-zero, players are kicked from the server if they are idle for more than that many minutes.

        [DefaultValue(1)]
        public uint Difficulty { get; set; }

        [DefaultValue(false)]
        public bool ForceGamemode { get; set; }

        [DefaultValue(0)]
        public uint Gamemode { get; set; }

        [DefaultValue(true)]
        public bool Pvp { get; set; }

        [DefaultValue(true)]
        public bool AnnouncePlayerAchievements { get; set; }

        // op
        [DefaultValue(false)]
        public bool WhiteList { get; set; }

        [DefaultValue(4)]
        public uint OpPermissionLevel { get; set; }
    }
}
