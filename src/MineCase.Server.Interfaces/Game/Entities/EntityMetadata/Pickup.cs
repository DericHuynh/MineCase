using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase.Server.Game.Entities.EntityMetadata
{
    [Orleans.GenerateSerializer]
    public class Pickup : Entity
    {
        [Orleans.Id(0)]
        public Slot Item { get; set; } = Slot.Empty;
    }
}
