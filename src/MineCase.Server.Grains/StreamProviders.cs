using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase.Server
{
    public static class StreamProviders
    {
        public const string MinecraftStreamProvider = "MinecraftStreamProvider";
        public const string PartitionedStreamProvider = "PartitioniedStreamProvider";
    }

    public static class Namespaces
    {
        public const string ChunkData = "chunk-data";
        public const string SessionCommands = "session-commands";
        public const string SessionPackets = "session-packets";
        public const string ClientIncoming = "client-incoming";

        public const string TickEmitter = "TickEmitter";
    }
}
