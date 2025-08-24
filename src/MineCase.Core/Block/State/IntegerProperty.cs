using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase.Block.State
{
    [Orleans.GenerateSerializer]
    public class IntegerProperty : StateProperty<int>
    {
        [Orleans.Id(0)]
        private int _max;

        public IntegerProperty(string name, int maxNumber)
        {
            Name = name;
            _max = maxNumber;
        }

        public override int StateNumber()
        {
            return _max;
        }

        public override int ToInt(int value)
        {
            if (value > _max)
                throw new ArgumentException("Value is greater than max number of the property.");

            return value;
        }
    }
}
