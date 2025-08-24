using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase.Util.Collections
{
    [Orleans.GenerateSerializer]
    public class ObjectIntDictionary<T>
    {
        [Orleans.Id(0)]
        private Dictionary<T, int> _dict = new Dictionary<T, int>();

        public void Add(T obj, int value)
        {
            _dict.Add(obj, value);
        }
    }
}
