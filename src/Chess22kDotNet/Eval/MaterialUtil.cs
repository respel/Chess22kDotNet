using System.Numerics;
using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet.Eval
{
    public static class MaterialUtil
    {
        public const int ScoreUnknown = 7777;

        private const int KPK = 0x00000001;
        private const int KPK_ = 0x00010000;
        private const int KRKP = 0x00010400;
        private const int KRKP_ = 0x04000001;
        private const int KQKP = 0x00012000;
        private const int KQKP_ = 0x20000001;
        private const int KRKR = 0x04000400;
        private const int KQKQ = 0x20002000;
        private const int KRKB = 0x00800400;
        private const int KRKB_ = 0x04000080;
        private const int KRNKR = 0x04000410;
        private const int KRNKR_ = 0x04100400;
        private const int KRBKB = 0x00800480;
        private const int KRBKB_ = 0x04800080;
        private const int KRBKR = 0x04000480;
        private const int KRBKR_ = 0x04800400;
        private const int KRKN = 0x00100400;
        private const int KRKN_ = 0x04000010;
        private const int KBNK = 0x00000090;
        private const int KBNK_ = 0x00900000;
        private const int KBPKP = 0x00010081;
        private const int KBPKP_ = 0x00810001;
        private const int KBPK = 0x00000081;

        private const int KBPK_ = 0x00810000;

        public static readonly int[][] Values =
        {
            // WHITE QQQRRRBBBNNNPPPP
            new[] {0, 1 << 0, 1 << 4, 1 << 7, 1 << 10, 1 << 13},
            // BLACK QQQRRRBBBNNNPPPP
            new[] {0, 1 << 16, 1 << 20, 1 << 23, 1 << 26, 1 << 29}
        };

        private static readonly int[] Shift = {0, 16};

        private const int MaskMinorMajorAll = -983056;
        private const int MaskMinorMajorWhite = 0xfff0;
        private const int MaskMinorMajorBlack = -1048576;
        private static readonly int[] MaskMinorMajor = {MaskMinorMajorWhite, MaskMinorMajorBlack};
        private static readonly int[] MaskNonNights = {0xff8f, -7405568};
        private const int MaskSingleBishops = 0x800080;
        private const int MaskSingleBishopNightWhite = KBNK;
        private const int MaskSingleBishopNightBlack = KBNK_;
        private static readonly int[] MaskPawnsQueens = {0xe00f, -535887872};
        private const int MaskPawns = 0xf000f;
        private static readonly int[] MaskSlidingPieces = {0xff80, -8388608};

        public static void SetKey(ChessBoard cb)
        {
            cb.MaterialKey = 0;
            for (var color = White; color <= Black; color++)
            {
                for (var pieceType = Pawn; pieceType <= Queen; pieceType++)
                {
                    cb.MaterialKey += BitOperations.PopCount((ulong) cb.Pieces[color][pieceType]) *
                                      Values[color][pieceType];
                }
            }
        }

        public static bool ContainsMajorPieces(int material)
        {
            return (material & MaskMinorMajorAll) != 0;
        }

        public static bool HasNonPawnPieces(int material, int color)
        {
            return (material & MaskMinorMajor[color]) != 0;
        }

        public static bool HasWhiteNonPawnPieces(int material)
        {
            return (material & MaskMinorMajorWhite) != 0;
        }

        public static bool HasBlackNonPawnPieces(int material)
        {
            return (material & MaskMinorMajorBlack) != 0;
        }

        public static bool OppositeBishops(int material)
        {
            return BitOperations.PopCount((ulong) (material & MaskMinorMajorAll)) == 2 &&
                   BitOperations.PopCount((ulong) (material & MaskSingleBishops)) == 2;
        }

        public static bool onlyWhitePawnsOrOneNightOrBishop(int material)
        {
            return BitOperations.PopCount((ulong) (material & MaskMinorMajorWhite)) switch
            {
                0 => true,
                1 => BitOperations.PopCount((ulong) (material & MaskSingleBishopNightWhite)) == 1,
                _ => false
            };
        }

        public static bool onlyBlackPawnsOrOneNightOrBishop(int material)
        {
            return BitOperations.PopCount((ulong) (material & MaskMinorMajorBlack)) switch
            {
                0 => true,
                1 => BitOperations.PopCount((ulong) (material & MaskSingleBishopNightBlack)) == 1,
                _ => false
            };
        }

        public static bool HasPawns(int material)
        {
            return (material & MaskPawns) != 0;
        }

        public static bool HasPawnsOrQueens(int material, int color)
        {
            return (material & MaskPawnsQueens[color]) != 0;
        }

        public static bool hasOnlyNights(int material, int color)
        {
            return (material & MaskNonNights[color]) == 0;
        }

        public static int GetMajorPieces(int material, int color)
        {
            return Util.RightTripleShift(material & MaskMinorMajor[color], Shift[color]);
        }

        public static bool HasSlidingPieces(int material, int color)
        {
            return (material & MaskSlidingPieces[color]) != 0;
        }

        public static bool IsKpk(int material)
        {
            return material == KPK || material == KPK_;
        }

        public static bool IsKbpk(int material)
        {
            return material == KBPK || material == KBPK_;
        }

        public static bool IsKbpkp(int material)
        {
            return material == KBPKP || material == KBPKP_;
        }

        public static bool IsKbnk(int material)
        {
            return material == KBNK || material == KBNK_;
        }

        public static bool IsKrkn(int material)
        {
            return material == KRKN || material == KRKN_;
        }

        public static bool IsKrkb(int material)
        {
            return material == KRKB || material == KRKB_;
        }

        public static bool IsKrbkb(int material)
        {
            return material == KRBKB || material == KRBKB_;
        }

        public static bool IsKrbkr(int material)
        {
            return material == KRBKR || material == KRBKR_;
        }

        public static bool IsKqkp(int material)
        {
            return material == KQKP || material == KQKP_;
        }

        public static bool IsKrkp(int material)
        {
            return material == KRKP || material == KRKP_;
        }

        public static bool IsDrawByMaterial(ChessBoard cb)
        {
            switch (cb.MaterialKey)
            {
                case 0x0: // KK
                case 0x10: // KNK
                case 0x20: // KNNK
                case 0x80: // KBK
                case 0x100000: // KKN
                case 0x100010: // KNKN
                case 0x100080: // KNKB
                case 0x200000: // KKNN
                case 0x800000: // KKB
                case 0x800010: // KBKN
                case 0x800080: // KBKB
                    return true;
                case KPK: // KPK
                case KPK_: // KPK
                    return KpkBitbase.IsDraw(cb);
                case KBPK: // KBPK
                case KBPK_: // KBPK
                    return EndGameEvaluator.IsKbpkDraw(cb.Pieces);
                case KBPKP: // KBPKP
                case KBPKP_: // KBPKP
                    return EndGameEvaluator.IsKbpkpDraw(cb.Pieces);
            }

            return false;
        }

        public static int CalculateEndgameScore(ChessBoard cb)
        {
            switch (cb.MaterialKey)
            {
                case KRKR:
                case KQKQ:
                    return EvalConstants.ScoreDraw;
                case KBNK:
                case KBNK_:
                    return EndGameEvaluator.CalculateKbnkScore(cb);
                case KRKN:
                case KRKN_:
                    return EndGameEvaluator.CalculateKrknScore(cb);
                case KQKP:
                case KQKP_:
                    if (EndGameEvaluator.IsKqkpDrawish(cb))
                    {
                        return cb.Pieces[White][Queen] == 0 ? -50 : 50;
                    }

                    return ScoreUnknown;
                case KRKP:
                case KRKP_:
                    if (EndGameEvaluator.IsKrkpDrawish(cb))
                    {
                        return cb.Pieces[White][Rook] == 0 ? -50 : 50;
                    }

                    return ScoreUnknown;

                case KRKB:
                case KRKB_:
                    return EndGameEvaluator.CalculateKrkbScore(cb);
                case KRNKR:
                case KRBKR:
                    return EndGameEvaluator.CalculateKingCorneredScore(cb, White);
                case KRNKR_:
                case KRBKR_:
                    return EndGameEvaluator.CalculateKingCorneredScore(cb, Black);
            }

            return ScoreUnknown;
        }
    }
}