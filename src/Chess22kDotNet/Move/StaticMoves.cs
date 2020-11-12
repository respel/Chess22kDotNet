using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet.Move
{
    public static class StaticMoves
    {
        public static readonly long[] KnightMoves = new long[64];
        public static readonly long[] KingMoves = new long[64];

        public static readonly long[][] PawnAttacks = Util.CreateJaggedArray<long[][]>(2, 64);

        // PAWN
        static StaticMoves()
        {
            for (var currentPosition = 0; currentPosition < 64; currentPosition++)
            {
                for (var newPosition = 0; newPosition < 64; newPosition++)
                {
                    // attacks
                    if (newPosition == currentPosition + 7 && newPosition % 8 != 7)
                    {
                        PawnAttacks[White][currentPosition] |= Util.PowerLookup[newPosition];
                    }

                    if (newPosition == currentPosition + 9 && newPosition % 8 != 0)
                    {
                        PawnAttacks[White][currentPosition] |= Util.PowerLookup[newPosition];
                    }

                    if (newPosition == currentPosition - 7 && newPosition % 8 != 0)
                    {
                        PawnAttacks[Black][currentPosition] |= Util.PowerLookup[newPosition];
                    }

                    if (newPosition == currentPosition - 9 && newPosition % 8 != 7)
                    {
                        PawnAttacks[Black][currentPosition] |= Util.PowerLookup[newPosition];
                    }
                }
            }

            // knight
            for (var currentPosition = 0; currentPosition < 64; currentPosition++)
            {
                for (var newPosition = 0; newPosition < 64; newPosition++)
                {
                    // check if newPosition is a correct move
                    if (IsKnightMove(currentPosition, newPosition))
                    {
                        KnightMoves[currentPosition] |= Util.PowerLookup[newPosition];
                    }
                }
            }

            // king
            for (var currentPosition = 0; currentPosition < 64; currentPosition++)
            {
                for (var newPosition = 0; newPosition < 64; newPosition++)
                {
                    // check if newPosition is a correct move
                    if (IsKingMove(currentPosition, newPosition))
                    {
                        KingMoves[currentPosition] |= Util.PowerLookup[newPosition];
                    }
                }
            }
        }

        private static bool IsKnightMove(int currentPosition, int newPosition)
        {
            if (currentPosition / 8 - newPosition / 8 == 1)
            {
                return currentPosition - 10 == newPosition || currentPosition - 6 == newPosition;
            }

            if (newPosition / 8 - currentPosition / 8 == 1)
            {
                return currentPosition + 10 == newPosition || currentPosition + 6 == newPosition;
            }

            if (currentPosition / 8 - newPosition / 8 == 2)
            {
                return currentPosition - 17 == newPosition || currentPosition - 15 == newPosition;
            }

            if (newPosition / 8 - currentPosition / 8 == 2)
            {
                return currentPosition + 17 == newPosition || currentPosition + 15 == newPosition;
            }

            return false;
        }

        private static bool IsKingMove(int currentPosition, int newPosition)
        {
            return (currentPosition / 8 - newPosition / 8) switch
            {
                0 => currentPosition - newPosition == -1 || currentPosition - newPosition == 1,
                1 => currentPosition - newPosition == 7 || currentPosition - newPosition == 8 ||
                     currentPosition - newPosition == 9,
                -1 => currentPosition - newPosition == -7 || currentPosition - newPosition == -8 ||
                      currentPosition - newPosition == -9,
                _ => false
            };
        }
    }
}