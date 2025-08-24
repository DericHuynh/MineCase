using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using MineCase.Server.Game;
using Newtonsoft.Json;
using Orleans;
using Orleans.CodeGeneration;

namespace MineCase.Server.Settings
{
    [GenerateSerializer]
    [JsonObject(MemberSerialization.OptOut)]
    public class ServerSettings
    {
        // server
        [DefaultValue("A Minecraft Server")]
        [Id(0)]
        public string Motd { get; set; } // this is the message that is displayed in the server list of the client

        [DefaultValue("127.0.0.1")]
        [Id(1)]
        public string ServerIp { get; set; }

        [DefaultValue(25565)]
        [Id(2)]
        public uint ServerPort { get; set; }

        [DefaultValue(60000)]
        [Id(3)]
        public uint MaxTickTime { get; set; }

        [DefaultValue(false)]
        [Id(4)]
        public bool OnlineMode { get; set; }

        [DefaultValue(false)]
        [Id(5)]
        public bool EnableRcon { get; set; } // Enables remote access to the server console.

        /// <summary>
        /// Gets or sets by default it allows packets that are n-1 bytes big to go normally, but a packet that n bytes or more will be compressed down. So, lower number means more compression but compressing small amounts of bytes might actually end up with a larger result than what went in.
        /// -1 - disable compression entirely
        /// 0+ - compress everything bigger than or equal to N.
        /// </summary>
        [DefaultValue(256)]
        [Id(6)]
        public uint NetworkCompressionThreshold { get; set; }

        [DefaultValue("")]
        [Id(7)]
        public string ResourcePack { get; set; }

        [DefaultValue("")]
        [Id(8)]
        public string ResourcePackHash { get; set; }

        [DefaultValue(true)]
        [Id(9)]
        public bool SnooperEnabled { get; set; }

        [DefaultValue(false)]
        [Id(10)]
        public bool EnableQuery { get; set; }

        // spawn
        [DefaultValue(16)]
        [Id(11)]
        public uint SpawnProtection { get; set; }

        [DefaultValue(true)]
        [Id(12)]
        public bool SpawnMonsters { get; set; }

        [DefaultValue(true)]
        [Id(13)]
        public bool SpawnNpcs { get; set; }

        [DefaultValue(true)]
        [Id(14)]
        public bool SpawnAnimals { get; set; }

        // world
        [DefaultValue(29999984)]
        [Id(15)]
        public uint MaxWorldSize { get; set; }

        [DefaultValue(true)]
        [Id(16)]
        public bool AllowNether { get; set; }

        [DefaultValue(true)]
        [Id(17)]
        public bool GenerateStructures { get; set; }

        [DefaultValue("")]
        [Id(18)]
        public string GeneratorSettings { get; set; }

        [DefaultValue(256)]
        [Id(19)]
        public uint MaxBuildHeight { get; set; }

        [DefaultValue("")]
        [Id(20)]
        public string LevelSeed { get; set; } // seed for you world

        [DefaultValue("DEFAULT")]
        [Id(21)]
        public string LevelType { get; set; } // type of you world

        [DefaultValue(false)]
        [Id(22)]
        public bool EnableCommandBlock { get; set; } // type of you world

        [DefaultValue(false)]
        [Id(23)]
        public bool Hardcore { get; set; } // If set to true, players will be set to spectator mode if they die.

        // user
        [DefaultValue(100)]
        [Id(24)]
        public uint MaxPlayers { get; set; }

        [DefaultValue(10)]
        [Id(25)]
        public uint ViewDistance { get; set; }

        [DefaultValue(false)]
        [Id(26)]
        public bool AllowFlight { get; set; }

        [DefaultValue(0)]
        [Id(27)]
        public uint PlayerIdleTimeout { get; set; } // If non-zero, players are kicked from the server if they are idle for more than that many minutes.

        [DefaultValue(1)]
        [Id(28)]
        public uint Difficulty { get; set; }

        [DefaultValue(false)]
        [Id(29)]
        public bool ForceGamemode { get; set; }

        [DefaultValue(0)]
        [Id(30)]
        public uint Gamemode { get; set; }

        [DefaultValue(true)]
        [Id(31)]
        public bool Pvp { get; set; }

        [DefaultValue(true)]
        [Id(32)]
        public bool AnnouncePlayerAchievements { get; set; }

        // op
        [DefaultValue(false)]
        [Id(33)]
        public bool WhiteList { get; set; }

        [DefaultValue(4)]
        [Id(34)]
        public uint OpPermissionLevel { get; set; }
    }
}
