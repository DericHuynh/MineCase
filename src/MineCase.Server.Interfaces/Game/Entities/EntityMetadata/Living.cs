using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase.Server.Game.Entities.EntityMetadata
{
    [Orleans.GenerateSerializer]
    public class Living : Entity
    {
        [Orleans.Id(0)]
        public bool IsHandActive { get; set; }

        [Orleans.Id(1)]
        public SwingHandState ActiveHand { get; set; }

        [Orleans.Id(2)]
        public float Health { get; set; }

        [Orleans.Id(3)]
        public int PotionEffectColor { get; set; }

        [Orleans.Id(4)]
        public bool IsPotionEffectAmbient { get; set; }

        [Orleans.Id(5)]
        public int NumberOfArrows { get; set; }
    }
}
