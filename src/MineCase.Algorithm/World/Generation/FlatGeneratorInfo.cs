using MineCase.Block;

namespace MineCase.Algorithm.World.Generation
{
    [Orleans.GenerateSerializer]
    public class FlatGeneratorInfo
    {
        [Orleans.Id(0)]
        public BlockState?[] FlatBlockId { get; set; }
    }
}