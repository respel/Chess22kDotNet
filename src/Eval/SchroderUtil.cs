namespace Chess22kDotNet.Eval
{
    public static class SchroderUtil
    {
        private const int FlagPawn = 1 << (ChessConstants.Pawn - 1);
        private const int FlagNight = 1 << (ChessConstants.Knight - 1);
        private const int FlagBishop = 1 << (ChessConstants.Bishop - 1);
        private const int FlagRook = 1 << (ChessConstants.Rook - 1);
        private const int FlagQueen = 1 << (ChessConstants.Queen - 1);

        public static readonly int[] Flags = {0, FlagPawn, FlagNight, FlagBishop, FlagRook, FlagQueen};
    }
}