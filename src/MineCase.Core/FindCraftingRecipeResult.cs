using System;
using System.Collections.Generic;
using System.Text;

namespace MineCase
{
    [Orleans.GenerateSerializer]
    public class FindCraftingRecipeResult
    {
        [Orleans.Id(0)]
        public Slot Result { get; set; }

        [Orleans.Id(1)]
        public Slot[,] AfterTake { get; set; }
    }
}
