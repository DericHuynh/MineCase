namespace MineCase.Block
{
    [Orleans.GenerateSerializer]
    public class SoundType
    {
        [Orleans.Id(0)]
        public float Volume { get; set; }

        [Orleans.Id(1)]
        public float Pitch { get; set; }

        /** The sound played when a block gets broken. */
        /** private SoundEvent breakSound { get; set } */
        /** The sound played when walking on a block. */
        /** private SoundEvent stepSound{ get;set } */
        /** The sound played when a block gets placed. */
        /** private SoundEvent placeSound{ get;set} */
        /** The sound played when a block gets hit (i.e. while mining). */
        /** private SoundEvent hitSound { get; set } */
        /** The sound played when a block gets fallen upon. */
        /** private SoundEvent fallSound { get; set } */
    }
}