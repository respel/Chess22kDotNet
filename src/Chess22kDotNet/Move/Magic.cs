namespace Chess22kDotNet.Move
{
    public class Magic
    {
        public Magic(long magicNumber)
        {
            MagicNumber = magicNumber;
        }

        public long MovementMask { get; set; }
        public long MagicNumber { get; }
        public int Shift { get; set; }
        public long[] MagicMoves { get; set; }
    }
}