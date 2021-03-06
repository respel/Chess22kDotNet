using System;
using System.Numerics;
using Chess22kDotNet.Engine;
using Chess22kDotNet.Move;
using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet.Eval
{
    public static class SeeUtil
    {
        private static int GetSmallestAttackSeeMove(long[] pieces, int colorToMove, int toIndex,
            long allPieces, long slidingMask)
        {
            // TODO stop when bad-capture

            // put 'super-piece' in see position

            // pawn non-promotion attacks
            var attackMove = StaticMoves.PawnAttacks[1 - colorToMove][toIndex] & pieces[Pawn] & allPieces;
            if (attackMove != 0) return BitOperations.TrailingZeroCount(attackMove);

            // knight attacks
            attackMove = pieces[Knight] & StaticMoves.KnightMoves[toIndex] & allPieces;
            if (attackMove != 0) return BitOperations.TrailingZeroCount(attackMove);

            // bishop attacks
            if ((pieces[Bishop] & slidingMask) != 0)
            {
                attackMove = pieces[Bishop] & MagicUtil.GetBishopMoves(toIndex, allPieces) & allPieces;
                if (attackMove != 0) return BitOperations.TrailingZeroCount(attackMove);
            }

            // rook attacks
            if ((pieces[Rook] & slidingMask) != 0)
            {
                attackMove = pieces[Rook] & MagicUtil.GetRookMoves(toIndex, allPieces) & allPieces;
                if (attackMove != 0) return BitOperations.TrailingZeroCount(attackMove);
            }

            // queen attacks
            if ((pieces[Queen] & slidingMask) != 0)
            {
                attackMove = pieces[Queen] & MagicUtil.GetQueenMoves(toIndex, allPieces) & allPieces;
                if (attackMove != 0) return BitOperations.TrailingZeroCount(attackMove);
            }

            // king attacks
            attackMove = pieces[King] & StaticMoves.KingMoves[toIndex];
            if (attackMove != 0) return BitOperations.TrailingZeroCount(attackMove);

            return -1;
        }

        private static int GetSeeScore(ChessBoard cb, int colorToMove, int toIndex, int attackedPieceIndex,
            long allPieces,
            long slidingMask)
        {
            if (Statistics.Enabled) Statistics.SeeNodes++;

            var fromIndex =
                GetSmallestAttackSeeMove(cb.Pieces[colorToMove], colorToMove, toIndex, allPieces, slidingMask);

            /* skip if the square isn't attacked anymore by this side */
            if (fromIndex == -1) return 0;

            if (attackedPieceIndex == King) return 3000;

            allPieces ^= Util.PowerLookup[fromIndex];
            slidingMask &= allPieces;

            /* Do not consider captures if they lose material, therefore max zero */
            return Math.Max(0,
                EvalConstants.Material[attackedPieceIndex] - GetSeeScore(cb, 1 - colorToMove, toIndex,
                    cb.PieceIndexes[fromIndex], allPieces, slidingMask));
        }

        public static int GetSeeCaptureScore(ChessBoard cb, int move)
        {
            if (EngineConstants.Assert)
                if (MoveUtil.GetAttackedPieceIndex(move) == 0)
                    Assert.IsTrue(MoveUtil.GetMoveType(move) != 0);

            var index = MoveUtil.GetToIndex(move);
            var allPieces = cb.AllPieces & ~Util.PowerLookup[MoveUtil.GetFromIndex(move)];
            var slidingMask = MagicUtil.GetQueenMovesEmptyBoard(index) & allPieces;

            // add score when promotion
            if (MoveUtil.IsPromotion(move))
                return EvalConstants.PromotionScore[MoveUtil.GetMoveType(move)] +
                       EvalConstants.Material[MoveUtil.GetAttackedPieceIndex(move)]
                       - GetSeeScore(cb, cb.ColorToMoveInverse, index, MoveUtil.GetMoveType(move), allPieces,
                           slidingMask);

            return EvalConstants.Material[MoveUtil.GetAttackedPieceIndex(move)]
                   - GetSeeScore(cb, cb.ColorToMoveInverse, index, MoveUtil.GetSourcePieceIndex(move), allPieces,
                       slidingMask);
        }
    }
}