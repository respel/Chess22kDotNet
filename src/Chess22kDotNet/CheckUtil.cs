using Chess22kDotNet.Move;
using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet
{
    public static class CheckUtil
    {
        public static bool IsInCheck(int kingIndex, int colorToMove, long[] enemyPieces, long allPieces)
        {
            // put 'super-piece' in kings position
            return (enemyPieces[Knight] & StaticMoves.KnightMoves[kingIndex]
                    | (enemyPieces[Rook] | enemyPieces[Queen]) & MagicUtil.GetRookMoves(kingIndex, allPieces)
                    | (enemyPieces[Bishop] | enemyPieces[Queen]) & MagicUtil.GetBishopMoves(kingIndex, allPieces)
                    | enemyPieces[Pawn] & StaticMoves.PawnAttacks[colorToMove][kingIndex]
                ) != 0;
        }

        public static bool IsInCheckIncludingKing(int kingIndex, int colorToMove, long[] enemyPieces,
            long allPieces)
        {
            // put 'super-piece' in kings position
            return (enemyPieces[Knight] & StaticMoves.KnightMoves[kingIndex]
                    | (enemyPieces[Rook] | enemyPieces[Queen]) & MagicUtil.GetRookMoves(kingIndex, allPieces)
                    | (enemyPieces[Bishop] | enemyPieces[Queen]) & MagicUtil.GetBishopMoves(kingIndex, allPieces)
                    | enemyPieces[Pawn] & StaticMoves.PawnAttacks[colorToMove][kingIndex]
                    | enemyPieces[King] & StaticMoves.KingMoves[kingIndex]
                ) != 0;
        }
    }
}