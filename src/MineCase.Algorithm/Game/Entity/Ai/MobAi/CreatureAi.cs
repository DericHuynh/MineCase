using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Server.World.EntitySpawner;
using MineCase.Server.World.EntitySpawner.Ai;
using MineCase.Server.World.EntitySpawner.Ai.Action;
using Stateless;

namespace MineCase.Algorithm.Game.Entity.Ai.MobAi
{
    [Orleans.GenerateSerializer]
    public abstract class CreatureAi
    {
        [Orleans.Id(0)]
        private readonly StateMachine<CreatureState, CreatureEvent> _stateMachine;

        public CreatureState State => _stateMachine.State;

        public CreatureAi(Func<CreatureState> getter, Action<CreatureState> setter)
        {
            var stateMachine = new StateMachine<CreatureState, CreatureEvent>(getter, setter);
            Configure(stateMachine);
            _stateMachine = stateMachine;
        }

        public void Fire(CreatureEvent @event) =>
            _stateMachine.Fire(@event);

        protected abstract void Configure(StateMachine<CreatureState, CreatureEvent> stateMachine);
    }
}
