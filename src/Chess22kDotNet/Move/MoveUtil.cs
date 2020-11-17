namespace Chess22kDotNet.Move
{
    public static class MoveUtil
    {
        // move types
        public const int TypeNormal = 0;
        public const int TypeEp = 1;
        public const int TypePromotionN = ChessConstants.Knight;
        public const int TypePromotionB = ChessConstants.Bishop;
        public const int TypePromotionR = ChessConstants.Rook;
        public const int TypePromotionQ = ChessConstants.Queen;
        public const int TypeCastling = 6;

        // shifts
        // ///////////////////// FROM //6 bits
        private const int ShiftTo = 6; // 6
        private const int ShiftSource = 12; // 3
        private const int ShiftAttack = 15; // 3
        private const int ShiftMoveType = 18; // 3
        private const int ShiftPromotion = 21; // 1

        // masks
        private const int Mask3Bits = 7; // 6
        private const int Mask6Bits = 0x3f; // 6
        private const int Mask12Bits = 0xfff;

        private const int MaskAttack = 7 << ShiftAttack; // 3
        private const int MaskPromotion = 1 << ShiftPromotion; // 1
        private const int MaskQuiet = MaskPromotion | MaskAttack;

        public static int GetFromIndex(int move)
        {
            return move & Mask6Bits;
        }

        public static int GetToIndex(int move)
        {
            return Util.RightTripleShift(move, ShiftTo) & Mask6Bits;
        }

        public static int GetFromToIndex(int move)
        {
            return move & Mask12Bits;
        }

        public static int GetAttackedPieceIndex(int move)
        {
            return Util.RightTripleShift(move, ShiftAttack) & Mask3Bits;
        }

        public static int GetSourcePieceIndex(int move)
        {
            return Util.RightTripleShift(move, ShiftSource) & Mask3Bits;
        }

        public static int GetMoveType(int move)
        {
            return Util.RightTripleShift(move, ShiftMoveType) & Mask3Bits;
        }

        public static int CreateMove(int fromIndex, int toIndex, int sourcePieceIndex)
        {
            return sourcePieceIndex << ShiftSource | toIndex << ShiftTo | fromIndex;
        }

        public static int CreateWhitePawnMove(int fromIndex)
        {
            return ChessConstants.Pawn << ShiftSource | (fromIndex + 8) << ShiftTo | fromIndex;
        }

        public static int CreateBlackPawnMove(int fromIndex)
        {
            return ChessConstants.Pawn << ShiftSource | (fromIndex - 8) << ShiftTo | fromIndex;
        }

        public static int CreateWhitePawn2Move(int fromIndex)
        {
            return ChessConstants.Pawn << ShiftSource | (fromIndex + 16) << ShiftTo | fromIndex;
        }

        public static int CreateBlackPawn2Move(int fromIndex)
        {
            return ChessConstants.Pawn << ShiftSource | (fromIndex - 16) << ShiftTo | fromIndex;
        }

        public static int CreatePromotionMove(int promotionPiece, int fromIndex, int toIndex)
        {
            return 1 << ShiftPromotion | promotionPiece << ShiftMoveType | ChessConstants.Pawn << ShiftSource |
                   toIndex << ShiftTo | fromIndex;
        }

        public static int CreateAttackMove(int fromIndex, int toIndex, int sourcePieceIndex,
            int attackedPieceIndex)
        {
            return attackedPieceIndex << ShiftAttack | sourcePieceIndex << ShiftSource | toIndex << ShiftTo | fromIndex;
        }

        public static int CreatePromotionAttack(int promotionPiece, int fromIndex, int toIndex,
            int attackedPieceIndex)
        {
            return 1 << ShiftPromotion | promotionPiece << ShiftMoveType | attackedPieceIndex << ShiftAttack |
                   ChessConstants.Pawn << ShiftSource
                   | toIndex << ShiftTo | fromIndex;
        }

        public static int CreateEpMove(int fromIndex, int toIndex)
        {
            return TypeEp << ShiftMoveType | ChessConstants.Pawn << ShiftAttack | ChessConstants.Pawn << ShiftSource |
                   toIndex << ShiftTo | fromIndex;
        }

        public static int CreateCastlingMove(int fromIndex, int toIndex)
        {
            return TypeCastling << ShiftMoveType | ChessConstants.King << ShiftSource | toIndex << ShiftTo | fromIndex;
        }

        public static bool IsPromotion(int move)
        {
            return (move & MaskPromotion) != 0;
        }

        public static bool IsPawnPush78(int move)
        {
            return GetSourcePieceIndex(move) == ChessConstants.Pawn && (GetToIndex(move) > 47 || GetToIndex(move) < 16);
        }

        /**
	    * no promotion and no attack
	    */
        public static bool IsQuiet(int move)
        {
            return (move & MaskQuiet) == 0;
        }

        public static bool IsNormalMove(int move)
        {
            return GetMoveType(move) == TypeNormal;
        }

        public static bool IsEpMove(int move)
        {
            return GetMoveType(move) == TypeEp;
        }

        public static bool IsCastlingMove(int move)
        {
            return GetMoveType(move) == TypeCastling;
        }
    }
}