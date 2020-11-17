using Chess22kDotNet.Move;

namespace Chess22kDotNet
{
    public static class ChessConstants
    {
        public const int CacheMiss = int.MinValue;

        public static readonly string[] FenWhitePieces = {"1", "P", "N", "B", "R", "Q", "K"};
        public static readonly string[] FenBlackPieces = {"1", "p", "n", "b", "r", "q", "k"};

        public const string FenStart = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public const int All = 0;
        public const int Empty = 0;
        public const int Pawn = 1;
        public const int Knight = 2;
        public const int Bishop = 3;
        public const int Rook = 4;
        public const int Queen = 5;
        public const int King = 6;

        public const int White = 0;
        public const int Black = 1;

        public const int ScoreNotRunning = 7777;

        public static readonly int[] ColorFactor = {1, -1};
        public static readonly int[] ColorFactor8 = {8, -8};

        public static readonly long[] KingArea = new long[64];

        public static readonly long[][] InBetween = Util.CreateJaggedArray<long[][]>(64, 64);

        /** pinned-piece index, king index */
        public static readonly long[][] PinnedMovement = Util.CreateJaggedArray<long[][]>(64, 64);

        static ChessConstants()
        {
            // fill from->to where to > from
            for (var from = 0; from < 64; from++)
            {
                for (var to = from + 1; to < 64; to++)
                {
                    // horizontal
                    int i;
                    if (from / 8 == to / 8)
                    {
                        i = to - 1;
                        while (i > from)
                        {
                            InBetween[from][to] |= Util.PowerLookup[i];
                            i--;
                        }
                    }

                    // vertical
                    if (from % 8 == to % 8)
                    {
                        i = to - 8;
                        while (i > from)
                        {
                            InBetween[from][to] |= Util.PowerLookup[i];
                            i -= 8;
                        }
                    }

                    // diagonal \
                    if ((to - from) % 9 == 0 && to % 8 > from % 8)
                    {
                        i = to - 9;
                        while (i > from)
                        {
                            InBetween[from][to] |= Util.PowerLookup[i];
                            i -= 9;
                        }
                    }

                    // diagonal /
                    if ((to - from) % 7 == 0 && to % 8 < from % 8)
                    {
                        i = to - 7;
                        while (i > from)
                        {
                            InBetween[from][to] |= Util.PowerLookup[i];
                            i -= 7;
                        }
                    }
                }
            }

            // fill from->to where to < from
            for (var from = 0; from < 64; from++)
            {
                for (var to = 0; to < from; to++)
                {
                    InBetween[from][to] = InBetween[to][from];
                }
            }

            int[] directions = {-1, -7, -8, -9, 1, 7, 8, 9};
            // PINNED MOVEMENT, x-ray from the king to the pinned-piece and beyond
            for (var pinnedPieceIndex = 0; pinnedPieceIndex < 64; pinnedPieceIndex++)
            {
                for (var kingIndex = 0; kingIndex < 64; kingIndex++)
                {
                    var correctDirection = 0;
                    foreach (var direction in directions)
                    {
                        if (correctDirection != 0)
                        {
                            break;
                        }

                        var xray = kingIndex + direction;
                        while (xray >= 0 && xray < 64)
                        {
                            if (direction == -1 || direction == -9 || direction == 7)
                            {
                                if ((xray & 7) == 7)
                                {
                                    break;
                                }
                            }

                            if (direction == 1 || direction == 9 || direction == -7)
                            {
                                if ((xray & 7) == 0)
                                {
                                    break;
                                }
                            }

                            if (xray == pinnedPieceIndex)
                            {
                                correctDirection = direction;
                                break;
                            }

                            xray += direction;
                        }
                    }

                    if (correctDirection == 0) continue;
                    {
                        var xray = kingIndex + correctDirection;
                        while (xray >= 0 && xray < 64)
                        {
                            if (correctDirection == -1 || correctDirection == -9 || correctDirection == 7)
                            {
                                if ((xray & 7) == 7)
                                {
                                    break;
                                }
                            }

                            if (correctDirection == 1 || correctDirection == 9 || correctDirection == -7)
                            {
                                if ((xray & 7) == 0)
                                {
                                    break;
                                }
                            }

                            PinnedMovement[pinnedPieceIndex][kingIndex] |= Util.PowerLookup[xray];
                            xray += correctDirection;
                        }
                    }
                }
            }

            // fill king-safety masks:
            //
            // FFF front
            // NKN next
            // BBB behind
            //
            for (var i = 0; i < 64; i++)
            {
                // NEXT
                KingArea[i] |= StaticMoves.KingMoves[i] | Util.PowerLookup[i];

                if (i > 55)
                {
                    KingArea[i] |= Util.RightTripleShift(StaticMoves.KingMoves[i], 8);
                }

                if (i < 8)
                {
                    KingArea[i] |= StaticMoves.KingMoves[i] << 8;
                }
            }

            // always 3 wide, even at file 1 and 8
            for (var i = 0; i < 64; i++)
            {
                switch (i % 8)
                {
                    case 0:
                        KingArea[i] |= KingArea[i + 1];
                        break;
                    case 7:
                        KingArea[i] |= KingArea[i - 1];
                        break;
                }
            }

            for (var i = 0; i < 64; i++)
            {
                KingArea[i] &= ~Util.PowerLookup[i];
            }
        }

        public enum ScoreType
        {
            Exact,
            Upper,
            Lower
        }
    }
}