namespace Chess22kDotNet.Move
{
    public class Magic
    {
        public long MovementMask { get; set; }
        public long MagicNumber { get; }
        public int Shift { get; set; }
        public long[] MagicMoves { get; set; }

        public Magic(long magicNumber)
        {
            MagicNumber = magicNumber;
        }
    }
}