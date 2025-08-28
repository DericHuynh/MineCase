using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IO;
using MineCase.Protocol;

namespace MineCase.Server.Network
{
    /// <summary>
    /// We can remove this interface and implement it into PacketSink, behind an abstract class and have the other sinks extend from it.
    /// </summary>
    [Orleans.GenerateSerializer]
    internal class PacketPackager : IPacketPackager
    {
        [Orleans.Id(0)]
        private readonly RecyclableMemoryStreamManager _memoryStreamMgr;

        public PacketPackager(RecyclableMemoryStreamManager memoryStreamMgr)
        {
            _memoryStreamMgr = memoryStreamMgr;
        }

        /// <summary>
        /// This isn't even asynchronous so this is quite useless. As a matter of fact any class that uses this can just extend an abstract class, although DI kinda makes sense aswell.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <returns>PacketId and data.</returns>
        public Task<(int PacketId, byte[] Data)> PreparePacket(IPacket packet)
        {
            using (var stream = _memoryStreamMgr.GetStream())
            {
                using (var bw = new BinaryWriter(stream, Encoding.UTF8, true))
                    packet.Serialize(bw);
                return Task.FromResult((GetPacketId(packet), stream.ToArray()));
            }
        }

        private int GetPacketId(IPacket packet)
        {
            var typeInfo = packet.GetType().GetTypeInfo();
            var attr = typeInfo.GetCustomAttribute<PacketAttribute>();
            return attr.PacketId;
        }
    }
}
