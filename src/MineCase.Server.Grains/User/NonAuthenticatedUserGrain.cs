using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Server.Persistence;
using MineCase.Server.Persistence.Components;
using Orleans;
using Orleans.Concurrency;

namespace MineCase.Server.User
{
    [PersistTableName("nonAuthenticatedUser")]
    [Reentrant]
    internal class NonAuthenticatedUserGrain : PersistableEntity, INonAuthenticatedUser
    {
        private StateHolder State => GetValue(StateComponent<StateHolder>.StateProperty);

        protected override void InitializePreLoadComponent()
        {
            SetComponent(new StateComponent<StateHolder>());
        }

        public Task<Guid> GetUUID() => Task.FromResult(State.UUID);

        public async Task<IUser> GetUser()
        {
            var user = GrainFactory.GetGrain<IUser>(State.UUID);
            await user.SetName(this.GetPrimaryKeyString());
            await user.SetProtocolVersion(State.ProtocolVersion);
            await WriteStateAsync();
            return user;
        }

        public Task<int> GetProtocolVersion()
        {
            return Task.FromResult(State.ProtocolVersion);
        }

        public Task SetProtocolVersion(int version)
        {
            State.ProtocolVersion = version;
            return Task.CompletedTask;
        }

        [Orleans.GenerateSerializer]
        internal class StateHolder
        {
            [Id(0)]
            public Guid UUID { get; set; }

            [Id(1)]
            public int ProtocolVersion { get; set; }

            public StateHolder()
            {
            }

            public StateHolder(InitializeStateMark mark)
            {
                UUID = Guid.NewGuid();
            }
        }
    }
}
