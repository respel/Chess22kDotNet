using System.Numerics;
using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet.Eval
{
    public static class EndGameEvaluator
    {
        public static int CalculateKingCorneredScore(ChessBoard cb, int leadingColor)
        {
            return Bitboard.ManhattanCenterDistance(cb.KingIndex[1 - leadingColor]) * ColorFactor[leadingColor];
        }

        public static int CalculateKbnkScore(ChessBoard cb)
        {
            if (BitOperations.PopCount((ulong)cb.Pieces[White][All]) > 1) return 1000 + CalculateKbnkScore(cb, White);

            return -1000 - CalculateKbnkScore(cb, Black);
        }

        private static int CalculateKbnkScore(ChessBoard cb, int color)
        {
            if ((cb.Pieces[color][Bishop] & Bitboard.WhiteSquares) != 0)
                return Bitboard.ManhattanCenterDistance(cb.KingIndex[1 - color]) * 100 *
                       ((Bitboard.WhiteCorners & cb.Pieces[1 - color][King]) != 0 ? 4 : 0);

            return Bitboard.ManhattanCenterDistance(cb.KingIndex[1 - color]) * 100 *
                   ((Bitboard.BlackCorners & cb.Pieces[1 - color][King]) != 0 ? 4 : 0);
        }

        public static int CalculateKrknScore(ChessBoard cb)
        {
            if (cb.Pieces[White][Rook] != 0)
                return Bitboard.ManhattanCenterDistance(cb.KingIndex[Black]) * 5 +
                       Util.GetDistance(cb.Pieces[Black][King], cb.Pieces[Black][Knight]) * 10;

            return -Bitboard.ManhattanCenterDistance(cb.KingIndex[White]) * 5 -
                   Util.GetDistance(cb.Pieces[White][King], cb.Pieces[White][Knight]) * 10;
        }

        public static int CalculateKrkbScore(ChessBoard cb)
        {
            if (cb.Pieces[White][Rook] != 0)
                return Bitboard.ManhattanCenterDistance(cb.KingIndex[Black]) * 2 + (cb.PinnedPieces == 0 ? 0 : 10);

            return -Bitboard.ManhattanCenterDistance(cb.KingIndex[White]) * 2 - (cb.PinnedPieces == 0 ? 0 : 10);
        }

        public static bool IsKrkpDrawish(ChessBoard cb)
        {
            var leadingColor = cb.Pieces[White][Rook] != 0 ? White : Black;
            var rook = cb.Pieces[leadingColor][Rook];
            var pawn = cb.Pieces[1 - leadingColor][Pawn];
            var pawnIndex = BitOperations.TrailingZeroCount(pawn);
            var winningKing = cb.Pieces[leadingColor][King];
            var losingKing = cb.Pieces[1 - leadingColor][King];

            if ((Bitboard.GetFile(pawn) & winningKing) != 0
                && (leadingColor == White && pawnIndex > cb.KingIndex[leadingColor] ||
                    leadingColor == Black && pawnIndex < cb.KingIndex[leadingColor]))
                // If the stronger side's king is in front of the pawn, it's a win
                return false;

            if (Util.GetDistance(losingKing, pawn) >= 3 + (cb.ColorToMove == 1 - leadingColor ? 1 : 0) &&
                Util.GetDistance(losingKing, rook) >= 3)
                // If the weaker side's king is too far from the pawn and the rook, it's a win.
                return false;

            if (leadingColor == White)
            {
                if (Bitboard.GetRank(losingKing) <= Bitboard.Rank3 && Util.GetDistance(losingKing, pawn) == 1 &&
                    Bitboard.GetRank(winningKing) >= Bitboard.Rank4
                    && Util.GetDistance(winningKing, pawn) > 2 + (cb.ColorToMove == leadingColor ? 1 : 0))
                    // If the pawn is far advanced and supported by the defending king, the position is drawish
                    return true;
            }
            else
            {
                if (Bitboard.GetRank(losingKing) >= Bitboard.Rank5 && Util.GetDistance(losingKing, pawn) == 1 &&
                    Bitboard.GetRank(winningKing) <= Bitboard.Rank5
                    && Util.GetDistance(winningKing, pawn) > 2 + (cb.ColorToMove == leadingColor ? 1 : 0))
                    // If the pawn is far advanced and supported by the defending king, the position is drawish
                    return true;
            }

            return false;
        }

        public static bool IsKqkpDrawish(ChessBoard cb)
        {
            var leadingColor = cb.Pieces[White][Queen] != 0 ? White : Black;
            var pawn = cb.Pieces[1 - leadingColor][Pawn];

            var ranks12 = leadingColor == White ? Bitboard.Rank12 : Bitboard.Rank78;
            long pawnZone;
            if ((Bitboard.FileA & pawn) != 0)
                pawnZone = Bitboard.FileAbc & ranks12;
            else if ((Bitboard.FileC & pawn) != 0)
                pawnZone = Bitboard.FileAbc & ranks12;
            else if ((Bitboard.FileF & pawn) != 0)
                pawnZone = Bitboard.FileFgh & ranks12;
            else if ((Bitboard.FileH & pawn) != 0)
                pawnZone = Bitboard.FileFgh & ranks12;
            else
                return false;

            if ((pawn & pawnZone) == 0) return false;

            if ((pawnZone & cb.Pieces[1 - leadingColor][King]) == 0) return false;
            return Util.GetDistance(cb.KingIndex[leadingColor], BitOperations.TrailingZeroCount(pawn)) >= 4;
        }

        public static bool IsKbpkDraw(long[][] pieces)
        {
            if (pieces[White][Bishop] != 0)
            {
                if ((pieces[White][Pawn] & Bitboard.FileA) != 0 && (Bitboard.WhiteSquares & pieces[White][Bishop]) == 0)
                    return (pieces[Black][King] & Bitboard.A7B7A8B8) != 0;

                if ((pieces[White][Pawn] & Bitboard.FileH) != 0 &&
                    (Bitboard.BlackSquares & pieces[White][Bishop]) == 0)
                    return (pieces[Black][King] & Bitboard.G7H7G8H8) != 0;
            }
            else
            {
                if ((pieces[Black][Pawn] & Bitboard.FileA) != 0 && (Bitboard.BlackSquares & pieces[Black][Bishop]) == 0)
                    return (pieces[White][King] & Bitboard.A1B1A2B2) != 0;

                if ((pieces[Black][Pawn] & Bitboard.FileH) != 0 &&
                    (Bitboard.WhiteSquares & pieces[Black][Bishop]) == 0)
                    return (pieces[White][King] & Bitboard.G1H1G2H2) != 0;
            }

            return false;
        }

        public static bool IsKbpkpDraw(long[][] pieces)
        {
            if (pieces[White][Bishop] != 0)
            {
                if ((pieces[Black][Pawn] & Bitboard.Rank5678) == 0) return false;
            }
            else
            {
                if ((pieces[White][Pawn] & Bitboard.Rank1234) == 0) return false;
            }

            return IsKbpkDraw(pieces);
        }
    }
}