using System;
using System.Collections.Generic;
using System.Text;
using MineCase.Server.Game.Entities;
using Orleans;

namespace MineCase.Server.World.EntitySpawner.Ai.Action
{
    internal class CreatureAiActionExplosion : CreatureAiAction
    {
        public CreatureAiActionExplosion()
            : base(CreatureState.Explosion)
        {
        }

        public override void Action(IMineCaseEntity creature)
        {
            throw new NotImplementedException();
        }
    }
}
