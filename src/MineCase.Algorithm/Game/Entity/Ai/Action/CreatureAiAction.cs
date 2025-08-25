using System;
using System.Collections.Generic;
using System.Text;
using MineCase.Server.Game.Entities;
using Orleans;

namespace MineCase.Server.World.EntitySpawner.Ai.Action
{
    [Orleans.GenerateSerializer]
    public abstract class CreatureAiAction
    {
        [Id(0)]
        public CreatureState State { get; set; }

        public CreatureAiAction(CreatureState state)
        {
            State = state;
        }

        public abstract void Action(IMineCaseEntity creature);
    }
}
