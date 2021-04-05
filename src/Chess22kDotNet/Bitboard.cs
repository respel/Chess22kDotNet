using System.Numerics;

namespace Chess22kDotNet
{
    public static class Bitboard
    {
        // rank 1
        public const long H1 = 1L;
        public const long G1 = H1 << 1;
        public const long F1 = G1 << 1;
        public const long E1 = F1 << 1;
        public const long D1 = E1 << 1;
        public const long C1 = D1 << 1;
        public const long B1 = C1 << 1;
        public const long A1 = B1 << 1;

        // rank 2
        public const long H2 = A1 << 1;
        public const long G2 = H2 << 1;
        public const long F2 = G2 << 1;
        public const long E2 = F2 << 1;
        public const long D2 = E2 << 1;
        public const long C2 = D2 << 1;
        public const long B2 = C2 << 1;
        public const long A2 = B2 << 1;

        // rank 3
        public const long H3 = A2 << 1;
        public const long G3 = H3 << 1;
        public const long F3 = G3 << 1;
        public const long E3 = F3 << 1;
        public const long D3 = E3 << 1;
        public const long C3 = D3 << 1;
        public const long B3 = C3 << 1;
        public const long A3 = B3 << 1;

        // rank 4
        public const long H4 = A3 << 1;
        public const long G4 = H4 << 1;
        public const long F4 = G4 << 1;
        public const long E4 = F4 << 1;
        public const long D4 = E4 << 1;
        public const long C4 = D4 << 1;
        public const long B4 = C4 << 1;
        public const long A4 = B4 << 1;

        // rank 5
        public const long H5 = A4 << 1;
        public const long G5 = H5 << 1;
        public const long F5 = G5 << 1;
        public const long E5 = F5 << 1;
        public const long D5 = E5 << 1;
        public const long C5 = D5 << 1;
        public const long B5 = C5 << 1;
        public const long A5 = B5 << 1;

        // rank 6
        public const long H6 = A5 << 1;
        public const long G6 = H6 << 1;
        public const long F6 = G6 << 1;
        public const long E6 = F6 << 1;
        public const long D6 = E6 << 1;
        public const long C6 = D6 << 1;
        public const long B6 = C6 << 1;
        public const long A6 = B6 << 1;

        // rank 7
        public const long H7 = A6 << 1;
        public const long G7 = H7 << 1;
        public const long F7 = G7 << 1;
        public const long E7 = F7 << 1;
        public const long D7 = E7 << 1;
        public const long C7 = D7 << 1;
        public const long B7 = C7 << 1;
        public const long A7 = B7 << 1;

        // rank 8
        public const long H8 = A7 << 1;
        public const long G8 = H8 << 1;
        public const long F8 = G8 << 1;
        public const long E8 = F8 << 1;
        public const long D8 = E8 << 1;
        public const long C8 = D8 << 1;
        public const long B8 = C8 << 1;
        public const long A8 = B8 << 1;

        // special squares
        public const long A1B1 = A1 | B1;
        public const long A1D1 = A1 | D1;
        public const long B1C1 = B1 | C1;
        public const long C1D1 = C1 | D1;
        public const long C1G1 = C1 | G1;
        public const long D1F1 = D1 | F1;
        public const long F1G1 = F1 | G1;
        public const long F1H1 = F1 | H1;
        public const long F1H8 = F1 | H8;
        public const long G1H1 = G1 | H1;
        public const long B3C2 = B3 | C2;
        public const long G3F2 = G3 | F2;
        public const long D4E5 = D4 | E5;
        public const long E4D5 = E4 | D5;
        public const long B6C7 = B6 | C7;
        public const long G6F7 = G6 | F7;
        public const long A8B8 = A8 | B8;
        public const long A8D8 = A8 | D8;
        public const long B8C8 = B8 | C8;
        public const long C8G8 = C8 | G8;
        public const long D8F8 = D8 | F8;
        public const long F8G8 = F8 | G8;
        public const long F8H8 = F8 | H8;
        public const long G8H8 = G8 | H8;
        public const long A1B1C1 = A1 | B1 | C1;
        public const long B1C1D1 = B1 | C1 | D1;
        public const long A8B8C8 = A8 | B8 | C8;
        public const long B8C8D8 = B8 | C8 | D8;
        public const long A1B1A2B2 = A1 | B1 | A2 | B2;
        public const long D1E1D2E2 = D1 | E1 | D2 | E2;
        public const long G1H1G2H2 = G1 | H1 | G2 | H2;
        public const long D7E7D8E8 = D7 | E7 | D8 | E8;
        public const long A7B7A8B8 = A7 | B7 | A8 | B8;
        public const long G7H7G8H8 = G7 | H7 | G8 | H8;
        public const long WhiteSquares = -0x55AA55AA55AA55ABL;
        public const long BlackSquares = ~WhiteSquares;
        public const long CornerSquares = A1 | H1 | A8 | H8;

        // ranks
        public const long Rank1 = A1 | B1 | C1 | D1 | E1 | F1 | G1 | H1;
        public const long Rank2 = A2 | B2 | C2 | D2 | E2 | F2 | G2 | H2;
        public const long Rank3 = A3 | B3 | C3 | D3 | E3 | F3 | G3 | H3;
        public const long Rank4 = A4 | B4 | C4 | D4 | E4 | F4 | G4 | H4;
        public const long Rank5 = A5 | B5 | C5 | D5 | E5 | F5 | G5 | H5;
        public const long Rank6 = A6 | B6 | C6 | D6 | E6 | F6 | G6 | H6;
        public const long Rank7 = A7 | B7 | C7 | D7 | E7 | F7 | G7 | H7;
        public const long Rank8 = A8 | B8 | C8 | D8 | E8 | F8 | G8 | H8;

        // special ranks
        public const long Rank12 = Rank1 | Rank2;
        public const long Rank78 = Rank7 | Rank8;
        public const long Rank123 = Rank1 | Rank2 | Rank3;
        public const long Rank234 = Rank2 | Rank3 | Rank4;
        public const long Rank567 = Rank5 | Rank6 | Rank7;
        public const long Rank678 = Rank6 | Rank7 | Rank8;
        public const long Rank1234 = Rank1 | Rank2 | Rank3 | Rank4;
        public const long Rank5678 = Rank5 | Rank6 | Rank7 | Rank8;
        public const long Rank23456 = Rank2 | Rank3 | Rank4 | Rank5 | Rank6;
        public const long Rank234567 = Rank2 | Rank3 | Rank4 | Rank5 | Rank6 | Rank7;
        public const long Rank34567 = Rank3 | Rank4 | Rank5 | Rank6 | Rank7;

        // files
        public const long FileA = A1 | A2 | A3 | A4 | A5 | A6 | A7 | A8;
        public const long FileB = B1 | B2 | B3 | B4 | B5 | B6 | B7 | B8;
        public const long FileC = C1 | C2 | C3 | C4 | C5 | C6 | C7 | C8;
        public const long FileD = D1 | D2 | D3 | D4 | D5 | D6 | D7 | D8;
        public const long FileE = E1 | E2 | E3 | E4 | E5 | E6 | E7 | E8;
        public const long FileF = F1 | F2 | F3 | F4 | F5 | F6 | F7 | F8;
        public const long FileG = G1 | G2 | G3 | G4 | G5 | G6 | G7 | G8;
        public const long FileH = H1 | H2 | H3 | H4 | H5 | H6 | H7 | H8;
        public const long FileAbc = FileA | FileB | FileC;
        public const long FileFgh = FileF | FileG | FileH;
        public const long FileCdef = FileC | FileD | FileE | FileF;
        public const long NotFileA = ~FileA;
        public const long NotFileH = ~FileH;

        // special
        public const long WhiteCorners = -0x70F1F3E7CF8F0E1L;
        public const long BlackCorners = 0x1f0f0783c1e0f0f8L;
        public static long[] RankPromotion = { Rank7, Rank2 };
        public static long[] RankNonPromotion = { ~RankPromotion[0], ~RankPromotion[1] };
        public static long[] RankFirst = { Rank1, Rank8 };

        public static readonly long[] Ranks = { Rank1, Rank2, Rank3, Rank4, Rank5, Rank6, Rank7, Rank8 };
        public static readonly long[] Files = { FileH, FileG, FileF, FileE, FileD, FileC, FileB, FileA };

        public static readonly long[] FilesAdjacent =
        {
            //
            FileG, //
            FileH | FileF, //
            FileG | FileE, //
            FileF | FileD, //
            FileE | FileC, //
            FileD | FileB, //
            FileC | FileA, //
            FileB
        };

        public static long GetWhitePawnAttacks(long pawns)
        {
            return ((pawns << 9) & NotFileH) | ((pawns << 7) & NotFileA);
        }

        public static long GetBlackPawnAttacks(long pawns)
        {
            return (Util.RightTripleShift(pawns, 9) & NotFileA) | (Util.RightTripleShift(pawns, 7) & NotFileH);
        }

        public static long GetPawnNeighbours(long pawns)
        {
            return ((pawns << 1) & NotFileH) | (Util.RightTripleShift(pawns, 1) & NotFileA);
        }

        /**
	    * @author Gerd Isenberg
	    */
        public static int ManhattanCenterDistance(int sq)
        {
            var file = sq & 7;
            var rank = Util.RightTripleShift(sq, 3);
            file ^= Util.RightTripleShift(file - 4, 8);
            rank ^= Util.RightTripleShift(rank - 4, 8);
            return (file + rank) & 7;
        }

        public static long GetWhitePassedPawnMask(int index)
        {
            if (index > 55) return 0;

            return (Files[index & 7] | FilesAdjacent[index & 7]) << ((Util.RightTripleShift(index, 3) << 3) + 8);
        }

        public static long GetBlackPassedPawnMask(int index)
        {
            return index < 8
                ? 0
                : Util.RightTripleShift(Files[index & 7] | FilesAdjacent[index & 7],
                    Util.RightTripleShift(71 - index, 3) << 3);
        }

        public static long GetWhiteAdjacentMask(int index)
        {
            return GetWhitePassedPawnMask(index) & ~Files[index & 7];
        }

        public static long GetBlackAdjacentMask(int index)
        {
            return GetBlackPassedPawnMask(index) & ~Files[index & 7];
        }

        public static long GetFile(long square)
        {
            return Files[BitOperations.TrailingZeroCount(square) & 7];
        }

        public static long GetRank(long square)
        {
            return Ranks[BitOperations.TrailingZeroCount(square) / 8];
        }
    }
}