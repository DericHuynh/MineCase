using System;
using System.Collections.Generic;
using System.Text;
using MineCase.Library;
using MineCase.World;
using Orleans;
using Orleans.Runtime;

namespace MineCase.Server.World
{
    public interface IAddressByPartition : IGrainWithStringKey
    {
    }

    public static class AddressByPartitionExtensions
    {
        public static string MakeAddressByPartitionKey(this IWorld world, ChunkWorldPos chunkWorldPos)
        {
            return $"{world.GetPrimaryKeyString()},{chunkWorldPos.X},{chunkWorldPos.Z}";
        }

        public static Guid MakeGuidByPartition(this IWorld world, ChunkWorldPos chunkWorldPos)
        {
            // World/Level int
            int hash = world.GetPrimaryKeyString().GetFNV1aHash();
            byte[] worldKeyBytes = BitConverter.GetBytes(hash);
            byte[] xBytes = BitConverter.GetBytes(chunkWorldPos.X);
            byte[] zBytes = BitConverter.GetBytes(chunkWorldPos.Z);
            byte[] bytes =
            [
                0, 0, 0, 0, .. worldKeyBytes, .. xBytes, .. zBytes
            ];

            return new Guid(bytes);
        }

        public static TGrainInterface GetPartitionGrain<TGrainInterface>(this IGrainFactory grainFactory, IWorld world, ChunkWorldPos chunkWorldPos)
            where TGrainInterface : IAddressByPartition
        {
            return grainFactory.GetGrain<TGrainInterface>(MakeAddressByPartitionKey(world, chunkWorldPos));
        }

        public static TGrainInterface GetPartitionGrain<TGrainInterface>(this IGrainFactory grainFactory, IAddressByPartition another)
            where TGrainInterface : IAddressByPartition
        {
            return grainFactory.GetGrain<TGrainInterface>(another.GetPrimaryKeyString());
        }

        public static ChunkWorldPos GetChunkWorldPos(this IAddressByPartition addressByPartition)
        {
            var key = addressByPartition.GetPrimaryKeyString().Split(',');
            return new ChunkWorldPos(int.Parse(key[1]), int.Parse(key[2]));
        }

        public static (string WorldKey, ChunkWorldPos ChunkWorldPos) GetWorldAndChunkWorldPos(this IAddressByPartition addressByPartition)
        {
            var key = addressByPartition.GetPrimaryKeyString().Split(',');
            return (key[0], new ChunkWorldPos(int.Parse(key[1]), int.Parse(key[2])));
        }
    }
}
