using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using MineCase.Serialization;

namespace MineCase.Protocol.Status
{
    [Packet(0x00)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class Request : IPacket
    {
        public static readonly Request Empty = new Request();

        public void Serialize(BinaryWriter bw)
        {
            // Seems like this does nothing
            return;
        }

        public void Deserialize(ref SpanReader br)
        {
            // Seems like this does nothing
            return;
        }
    }

    [Packet(0x00)]
    [Orleans.GenerateSerializer]
    [MineCase.Serialization.GenerateSerializer]
    public sealed class Response : IPacket
    {
        [SerializeAs(DataType.String)]
        [Orleans.Id(0)]
        public string JsonResponse;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsString(JsonResponse);
        }

        public void Deserialize(ref SpanReader br)
        {
            // Not Used
            JsonResponse = br.ReadAsString();
        }
    }
}
