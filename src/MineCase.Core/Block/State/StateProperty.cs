using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase.Block.State
{
    [Orleans.GenerateSerializer]
    public abstract class StateProperty<T> : IStateProperty
    {
        [Orleans.Id(0)]
        public string Name { get; set; }

        public string GetName()
        {
            return Name;
        }

        public abstract int StateNumber();

        public abstract int ToInt(T value);
    }
}
