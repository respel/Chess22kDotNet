namespace Chess22kDotNet.Move
{
    public class Magic
    {
        public long MovementMask;
        public readonly long MagicNumber;
        public int Shift;
        public long[] MagicMoves;

        public Magic(long magicNumber)
        {
            MagicNumber = magicNumber;
        }
    }
}