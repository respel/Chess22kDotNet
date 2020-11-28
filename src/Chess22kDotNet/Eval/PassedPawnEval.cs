using System;
using System.Numerics;
using Chess22kDotNet.Move;
using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet.Eval
{
    public static class PassedPawnEval
    {
        public static int CalculateScores(ChessBoard cb)
        {
            var score = 0;

            var whitePromotionDistance = Util.ShortMax;
            var blackPromotionDistance = Util.ShortMax;

            // white passed pawns
            var passedPawns = cb.PassedPawnsAndOutposts & cb.Pieces[White][Pawn];
            while (passedPawns != 0)
            {
                var index = 63 - BitOperations.LeadingZeroCount((ulong) passedPawns);

                score += GetPassedPawnScore(cb, index, White);

                if (whitePromotionDistance == Util.ShortMax)
                    whitePromotionDistance = GetWhitePromotionDistance(cb, index);

                // skip all passed pawns at same file
                passedPawns &= ~Bitboard.Files[index & 7];
            }

            // black passed pawns
            passedPawns = cb.PassedPawnsAndOutposts & cb.Pieces[Black][Pawn];
            while (passedPawns != 0)
            {
                var index = BitOperations.TrailingZeroCount(passedPawns);

                score -= GetPassedPawnScore(cb, index, Black);

                if (blackPromotionDistance == Util.ShortMax)
                    blackPromotionDistance = GetBlackPromotionDistance(cb, index);

                // skip all passed pawns at same file
                passedPawns &= ~Bitboard.Files[index & 7];
            }

            if (whitePromotionDistance < blackPromotionDistance - 1)
                score += 350;
            else if (whitePromotionDistance > blackPromotionDistance + 1) score -= 350;

            return score;
        }

        private static int GetPassedPawnScore(ChessBoard cb, int index, int color)
        {
            var nextIndex = index + ColorFactor8[color];
            var square = Util.PowerLookup[index];
            var maskNextSquare = Util.PowerLookup[nextIndex];
            var maskPreviousSquare = Util.PowerLookup[index - ColorFactor8[color]];
            var maskFile = Bitboard.Files[index & 7];
            var enemyColor = 1 - color;
            float multiplier = 1;

            // is piece blocked?
            if ((cb.AllPieces & maskNextSquare) != 0) multiplier *= EvalConstants.PassedMultipliers[0];

            // is next squared attacked?
            if ((cb.Attacks[enemyColor][All] & maskNextSquare) == 0)
            {
                // complete path free of enemy attacks?
                if ((PinnedMovement[nextIndex][index] & cb.Attacks[enemyColor][All]) == 0)
                    multiplier *= EvalConstants.PassedMultipliers[7];
                else
                    multiplier *= EvalConstants.PassedMultipliers[1];
            }

            // is next squared defended?
            if ((cb.Attacks[color][All] & maskNextSquare) != 0) multiplier *= EvalConstants.PassedMultipliers[3];

            // is enemy king in front?
            if ((PinnedMovement[nextIndex][index] & cb.Pieces[enemyColor][King]) != 0)
                multiplier *= EvalConstants.PassedMultipliers[2];

            // under attack?
            if (cb.ColorToMove != color && (cb.Attacks[enemyColor][All] & square) != 0)
                multiplier *= EvalConstants.PassedMultipliers[4];

            // defended by rook from behind?
            if ((maskFile & cb.Pieces[color][Rook]) != 0 && (cb.Attacks[color][Rook] & square) != 0 &&
                (cb.Attacks[color][Rook] & maskPreviousSquare) != 0)
                multiplier *= EvalConstants.PassedMultipliers[5];

            // attacked by rook from behind?
            else if ((maskFile & cb.Pieces[enemyColor][Rook]) != 0 && (cb.Attacks[enemyColor][Rook] & square) != 0
                                                                   && (cb.Attacks[enemyColor][Rook] &
                                                                       maskPreviousSquare) != 0)
                multiplier *= EvalConstants.PassedMultipliers[6];

            // king tropism
            multiplier *= EvalConstants.PassedKingMulti[Util.GetDistance(cb.KingIndex[color], index)];
            multiplier *= EvalConstants.PassedKingMulti[8 - Util.GetDistance(cb.KingIndex[enemyColor], index)];

            var scoreIndex = 7 * color + ColorFactor[color] * index / 8;
            return (int) (EvalConstants.PassedScoreEg[scoreIndex] * multiplier);
        }

        private static int GetBlackPromotionDistance(ChessBoard cb, int index)
        {
            // check if it cannot be stopped
            var promotionDistance = Util.RightTripleShift(index, 3);
            if (promotionDistance == 1 && cb.ColorToMove == Black)
            {
                if ((Util.PowerLookup[index - 8] & (cb.Attacks[White][All] | cb.AllPieces)) != 0) return Util.ShortMax;
                if ((Util.PowerLookup[index] & cb.Attacks[White][All]) == 0) return 1;
            }
            else if (MaterialUtil.onlyWhitePawnsOrOneNightOrBishop(cb.MaterialKey))
            {
                // check if it is my turn
                if (cb.ColorToMove == White) promotionDistance++;

                // check if own pieces are blocking the path
                if (BitOperations.TrailingZeroCount(cb.Pieces[Black][All] & Bitboard.Files[index & 7]) < index)
                    promotionDistance++;

                // check if own king is defending the promotion square (including square just below)
                if ((StaticMoves.KingMoves[cb.KingIndex[Black]] & KingArea[index] & Bitboard.Rank12) != 0)
                    promotionDistance--;

                // check distance of enemy king to promotion square
                if (promotionDistance >= Math.Max(Util.RightTripleShift(cb.KingIndex[White], 3),
                    Math.Abs((index & 7) - (cb.KingIndex[White] & 7)))) return Util.ShortMax;
                if (!MaterialUtil.HasWhiteNonPawnPieces(cb.MaterialKey)) return promotionDistance;

                if (cb.Pieces[White][Knight] != 0)
                {
                    // check distance of enemy night
                    if (promotionDistance <
                        Util.GetDistance(BitOperations.TrailingZeroCount(cb.Pieces[White][Knight]), index))
                        return promotionDistance;
                }
                else
                {
                    // can bishop stop the passed pawn?
                    if (Util.RightTripleShift(index, 3) != 1) return Util.ShortMax;
                    if ((Util.PowerLookup[index] & Bitboard.WhiteSquares) == 0 !=
                        ((cb.Pieces[White][Bishop] & Bitboard.WhiteSquares) == 0)) return Util.ShortMax;
                    if ((cb.Attacks[White][All] & Util.PowerLookup[index]) == 0) return promotionDistance;
                }
            }

            return Util.ShortMax;
        }

        private static int GetWhitePromotionDistance(ChessBoard cb, int index)
        {
            // check if it cannot be stopped
            var promotionDistance = 7 - index / 8;
            if (promotionDistance == 1 && cb.ColorToMove == White)
            {
                if ((Util.PowerLookup[index + 8] & (cb.Attacks[Black][All] | cb.AllPieces)) != 0) return Util.ShortMax;
                if ((Util.PowerLookup[index] & cb.Attacks[Black][All]) == 0) return 1;
            }
            else if (MaterialUtil.onlyBlackPawnsOrOneNightOrBishop(cb.MaterialKey))
            {
                // check if it is my turn
                if (cb.ColorToMove == Black) promotionDistance++;

                // check if own pieces are blocking the path
                if (63 - BitOperations.LeadingZeroCount((ulong) (cb.Pieces[White][All] & Bitboard.Files[index & 7])) >
                    index)
                    promotionDistance++;

                // TODO maybe the enemy king can capture the pawn!!
                // check if own king is defending the promotion square (including square just below)
                if ((StaticMoves.KingMoves[cb.KingIndex[White]] & KingArea[index] & Bitboard.Rank78) != 0)
                    promotionDistance--;

                // check distance of enemy king to promotion square
                if (promotionDistance >= Math.Max(7 - cb.KingIndex[Black] / 8,
                    Math.Abs((index & 7) - (cb.KingIndex[Black] & 7)))) return Util.ShortMax;
                if (!MaterialUtil.HasBlackNonPawnPieces(cb.MaterialKey)) return promotionDistance;

                if (cb.Pieces[Black][Knight] != 0)
                {
                    // check distance of enemy night
                    if (promotionDistance <
                        Util.GetDistance(BitOperations.TrailingZeroCount(cb.Pieces[Black][Knight]), index))
                        return promotionDistance;
                }
                else
                {
                    // can bishop stop the passed pawn?
                    if (Util.RightTripleShift(index, 3) != 6) return Util.ShortMax;
                    // rank 7
                    if ((Util.PowerLookup[index] & Bitboard.WhiteSquares) == 0 !=
                        ((cb.Pieces[Black][Bishop] & Bitboard.WhiteSquares) == 0)) return Util.ShortMax;
                    // other color than promotion square
                    if ((cb.Attacks[Black][All] & Util.PowerLookup[index]) == 0) return promotionDistance;
                }
            }

            return Util.ShortMax;
        }
    }
}