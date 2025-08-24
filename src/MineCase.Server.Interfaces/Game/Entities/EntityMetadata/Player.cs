using System;
using System.Collections.Generic;
using System.Text;
using MineCase.Nbt;

namespace MineCase.Server.Game.Entities.EntityMetadata
{
    [Orleans.GenerateSerializer]
    public class Player : Living
    {
        [Orleans.Id(0)]
        public float AdditionalHearts { get; set; }

        [Orleans.Id(1)]
        public uint Score { get; set; }

        [Orleans.Id(2)]
        public bool CapeEnabled { get; set; }

        [Orleans.Id(3)]
        public bool JacketEnabled { get; set; }

        [Orleans.Id(4)]
        public bool LeftSleeveEnabled { get; set; }

        [Orleans.Id(5)]
        public bool RightSleeveEnabled { get; set; }

        [Orleans.Id(6)]
        public bool LeftPantsLegEnabled { get; set; }

        [Orleans.Id(7)]
        public bool RightPantsLegEnabled { get; set; }

        [Orleans.Id(8)]
        public bool HatEnabled { get; set; }

        [Orleans.Id(9)]
        public byte MainHand { get; set; } = 1;

        [Orleans.Id(10)]
        public NbtFile LeftShoulder { get; set; }

        [Orleans.Id(11)]
        public NbtFile RightShoulder { get; set; }
    }
}
