using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineCase.Library
{
    public static class StringExtensions
    {
        public static int GetFNV1aHash(this string str)
        {
            const uint FNV_PRIME = 16777619;
            const uint FNV_OFFSET_BASIS = 2166136261;

            uint hash = FNV_OFFSET_BASIS;

            foreach (char c in str)
            {
                hash ^= (byte)c;
                hash *= FNV_PRIME;
            }

            return (int)hash;
        }
    }
}
