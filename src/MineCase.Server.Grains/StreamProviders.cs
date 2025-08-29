using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase.Server
{
    public static class StreamProviders
    {
        public const string MinecraftStreamProvider = "MinecraftStreamProvider";

        public static class Namespaces
        {
            public const string ChunkSender = "ChunkSender";

            public const string TickEmitter = "TickEmitter";
        }
    }
}
