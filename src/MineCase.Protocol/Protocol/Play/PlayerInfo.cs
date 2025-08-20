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
    [MineCase.Serialization.GenerateSerializer]
    public sealed class PlayerInfo<TAction> : IPacket
        where TAction : PlayerInfoAction, new()
    {
        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(0)]
        public uint Action;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(1)]
        public uint NumberOfPlayers;

        [SerializeAs(DataType.Array, ArrayLengthMember = nameof(NumberOfPlayers))]
        [Orleans.Id(2)]
        public TAction[] Players;

        public void Serialize(BinaryWriter bw)
        {
            bw.WriteAsVarInt(Action, out _);
            bw.WriteAsVarInt(NumberOfPlayers, out _);
            bw.WriteAsArray(Players);
        }

        public void Deserialize(ref SpanReader br)
        {
            Action = br.ReadAsVarInt(out _);
            NumberOfPlayers = br.ReadAsVarInt(out _);
            Players = br.ReadAsArray<TAction>((int)NumberOfPlayers);
        }
    }

    [Orleans.GenerateSerializer]
    public abstract class PlayerInfoAction : IPacket
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
    public sealed class PlayerInfoAddPlayerAction : PlayerInfoAction
    {
        [SerializeAs(DataType.String)]
        [Orleans.Id(0)]
        public string Name;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(1)]
        public uint NumberOfProperties;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(2)]
        public uint GameMode;

        [SerializeAs(DataType.VarInt)]
        [Orleans.Id(3)]
        public uint Ping;

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
