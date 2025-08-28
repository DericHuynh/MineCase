using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MineCase.Library
{
    public static class ListExtensions
    {
        /// <summary>
        /// Removes an item from a list without list order guarantees.
        /// Takes removes the item and replaces it with the element at the end of the list, removing the end.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool RemoveUnordered<T>(this List<T> list, T element)
        {
            int index = list.IndexOf(element);
            if (index < 0)
                return false;
            if ((uint)index >= (uint)list.Count)
                throw new ArgumentOutOfRangeException("RemoveUnordered Index out of range.");
            list[index] = list[^1];
            list.RemoveAt(list.Count - 1);
            return true;
        }
    }
}
