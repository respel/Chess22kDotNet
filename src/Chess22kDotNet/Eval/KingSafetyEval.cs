using System;
using System.Numerics;
using Chess22kDotNet.Move;
using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet.Eval
{
    public static class KingSafetyEval
    {
        public static int CalculateScores(ChessBoard cb)
        {
            var score = 0;

            for (var kingColor = White; kingColor <= Black; kingColor++)
            {
                var enemyColor = 1 - kingColor;

                if ((cb.Pieces[enemyColor][Rook] | cb.Pieces[enemyColor][Queen]) == 0) continue;

                var kingIndex = cb.KingIndex[kingColor];
                var counter = 0;

                var kingArea = KingArea[kingIndex];
                counter += EvalConstants.KsFriends[
                    BitOperations.PopCount((ulong) (kingArea & cb.Pieces[kingColor][All]))];
                counter += EvalConstants.KsAttacks[
                    BitOperations.PopCount((ulong) (kingArea & cb.Attacks[enemyColor][All]))];
                counter += EvalConstants.KsKnightDefenders[
                    BitOperations.PopCount((ulong) (kingArea & cb.Attacks[kingColor][Knight]))];
                counter += EvalConstants.KsWeak[BitOperations.PopCount((ulong) (kingArea & cb.Attacks[enemyColor][All]
                    & ~(cb.Attacks[kingColor][Pawn] |
                        cb.Attacks[kingColor][Knight] |
                        cb.Attacks[kingColor][Bishop] |
                        cb.Attacks[kingColor][Rook])))];
                counter += EvalConstants.KsDoubleAttacks[
                    BitOperations.PopCount((ulong) (kingArea & cb.DoubleAttacks[enemyColor] &
                                                    ~cb.Attacks[kingColor][Pawn]))];

                counter += Checks(cb, kingColor);

                // bonus for stm
                counter += (1 - cb.ColorToMove) ^ enemyColor;

                // bonus if there are discovered checks possible
                if (cb.DiscoveredPieces != 0)
                    counter += BitOperations.PopCount((ulong) (cb.DiscoveredPieces & cb.Pieces[enemyColor][All])) * 2;

                if (cb.Pieces[enemyColor][Queen] != 0)
                    // bonus for small king-queen distance
                    if ((cb.Attacks[kingColor][All] & cb.Pieces[enemyColor][Queen]) == 0)
                        counter += EvalConstants.KsQueenTropism[
                            Util.GetDistance(kingIndex, BitOperations.TrailingZeroCount(cb.Pieces[enemyColor][Queen]))];

                counter += EvalConstants.KsAttackPattern[cb.KingAttackersFlag[enemyColor]];
                score += ColorFactor[enemyColor] *
                         EvalConstants.KsScores[Math.Min(counter, EvalConstants.KsScores.Length - 1)];
            }

            return score;
        }

        private static int Checks(ChessBoard cb, int kingColor)
        {
            var kingIndex = cb.KingIndex[kingColor];
            var enemyColor = 1 - kingColor;
            var notDefended = ~cb.Attacks[kingColor][All];
            var unOccupied = ~cb.Pieces[enemyColor][All];
            var unsafeKingMoves = StaticMoves.KingMoves[kingIndex] & cb.DoubleAttacks[enemyColor] &
                                  ~cb.DoubleAttacks[kingColor];

            var counter = 0;
            if (cb.Pieces[enemyColor][Knight] != 0)
                counter += CheckMinor(notDefended,
                    StaticMoves.KnightMoves[kingIndex] & unOccupied & cb.Attacks[enemyColor][Knight]);

            long moves;
            long queenMoves = 0;
            if ((cb.Pieces[enemyColor][Queen] | cb.Pieces[enemyColor][Bishop]) != 0)
            {
                moves = MagicUtil.GetBishopMoves(kingIndex, cb.AllPieces ^ cb.Pieces[kingColor][Queen]) & unOccupied;
                queenMoves = moves;
                counter += CheckMinor(notDefended, moves & cb.Attacks[enemyColor][Bishop]);
            }

            if ((cb.Pieces[enemyColor][Queen] | cb.Pieces[enemyColor][Rook]) != 0)
            {
                moves = MagicUtil.GetRookMoves(kingIndex, cb.AllPieces ^ cb.Pieces[kingColor][Queen]) & unOccupied;
                queenMoves |= moves;
                counter += CheckRook(cb, kingColor, moves & cb.Attacks[enemyColor][Rook],
                    unsafeKingMoves | notDefended);
            }

            queenMoves &= cb.Attacks[enemyColor][Queen];
            if (queenMoves == 0) return counter;
            // safe check queen
            if ((queenMoves & notDefended) != 0)
                counter += EvalConstants.KsCheckQueen[BitOperations.PopCount((ulong) cb.Pieces[kingColor][All])];

            // safe check queen touch
            if ((queenMoves & unsafeKingMoves) != 0) counter += EvalConstants.KsOther[0];

            return counter;
        }

        private static int CheckRook(ChessBoard cb, int kingColor, long rookMoves, long safeSquares)
        {
            if (rookMoves == 0) return 0;

            if ((rookMoves & safeSquares) == 0) return EvalConstants.KsOther[3];

            var counter = EvalConstants.KsOther[2];
            if (KingBlockedAtLastRank(StaticMoves.KingMoves[cb.KingIndex[kingColor]] & cb.EmptySpaces &
                                      ~cb.Attacks[1 - kingColor][All]))
                counter += EvalConstants.KsOther[1];

            return counter;
        }

        private static int CheckMinor(long safeSquares, long bishopMoves)
        {
            if (bishopMoves == 0) return 0;

            return (bishopMoves & safeSquares) == 0 ? EvalConstants.KsOther[3] : EvalConstants.KsOther[2];
        }

        private static bool KingBlockedAtLastRank(long safeKingMoves)
        {
            return (Bitboard.Rank234567 & safeKingMoves) == 0;
        }
    }
}