using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineCase.Serialization;

namespace MineCase.Protocol.Play
{
    // In 1.12, it is PlayerListItem. Now it is PlayerInfo
    [Packet(0x34)]
    [Orleans.GenerateSerializer]
    [GenerateSerializer]
    public sealed partial class PlayerInfo<TAction> : IPacket
        where TAction : PlayerInfoAction, new()
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public int Action;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(1)]
        public int NumberOfPlayers;

        [SerializeAs(DataType.Array, ArrayLengthMember = nameof(NumberOfPlayers))]
        [Orleans.Id(2)]
        public TAction[] Players;
    }

    [Orleans.GenerateSerializer]
    public abstract partial class PlayerInfoAction : IPacket
    {
        [SerializeAs(DataType.UUID)]
        [Orleans.Id(0)]
        public Guid UUID;

        public virtual void Deserialize(ref SpanReader br)
        {
            UUID = br.ReadAsUUID();
        }

        public virtual void Serialize(BinaryWriter bw)
        {
            bw.WriteAsUUID(UUID);
        }
    }

    [Orleans.GenerateSerializer]
    public sealed partial class PlayerInfoAddPlayerAction : PlayerInfoAction
    {
        [SerializeAs(DataType.String)]
        [Orleans.Id(0)]
        public string Name;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(1)]
        public int NumberOfProperties;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(2)]
        public int GameMode;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(3)]
        public int Ping;

        [SerializeAs(DataType.Boolean)]
        [Orleans.Id(4)]
        public bool HasDisplayName;

        [SerializeAs(DataType.Chat)]
        [Orleans.Id(5)]
        public string DisplayName;

        public override void Deserialize(ref SpanReader br)
        {
            base.Deserialize(ref br);
        }

        public override void Serialize(BinaryWriter bw)
        {
            base.Serialize(bw);

            bw.WriteAsString(Name);
            bw.WriteAsVarInt(NumberOfProperties, out _);
            bw.WriteAsVarInt(GameMode, out _);
            bw.WriteAsVarInt(Ping, out _);
            bw.WriteAsBoolean(HasDisplayName);
            if (HasDisplayName)
                throw new NotImplementedException();
        }
    }

    public sealed class PlayerInfoRemovePlayerAction : PlayerInfoAction
    {
    }
}
