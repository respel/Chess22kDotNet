using System;
using System.Numerics;
using Chess22kDotNet.Eval;
using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet
{
    public static class CastlingUtil
    {
        // 4 bits: white-king,white-queen,black-king,black-queen
        public static long GetCastlingIndexes(in ChessBoard cb)
        {
            if (cb.CastlingRights == 0)
            {
                return 0;
            }

            if (cb.ColorToMove == White)
            {
                switch (cb.CastlingRights)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        return 0;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        return Bitboard.C1;
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                        return Bitboard.G1;
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        return Bitboard.C1G1;
                }
            }
            else
            {
                switch (cb.CastlingRights)
                {
                    case 0:
                    case 4:
                    case 8:
                    case 12:
                        return 0;
                    case 1:
                    case 5:
                    case 9:
                    case 13:
                        return Bitboard.C8;
                    case 2:
                    case 6:
                    case 10:
                    case 14:
                        return Bitboard.G8;
                    case 3:
                    case 7:
                    case 11:
                    case 15:
                        return Bitboard.C8G8;
                }
            }

            throw new ArgumentException("Unknown castling-right: " + cb.CastlingRights);
        }

        public static int GetRookMovedOrAttackedCastlingRights(in int castlingRights, in int rookIndex)
        {
            return rookIndex switch
            {
                0 => castlingRights & 7 // 0111
                ,
                7 => castlingRights & 11 // 1011
                ,
                56 => castlingRights & 13 // 1101
                ,
                63 => castlingRights & 14 // 1110
                ,
                _ => castlingRights
            };
        }

        public static int GetKingMovedCastlingRights(in int castlingRights, in int kingIndex)
        {
            return kingIndex switch
            {
                // 0011
                3 => castlingRights & 3,
                // 1100
                59 => castlingRights & 12,
                _ => castlingRights
            };
        }

        private static long GetRookInBetweenIndex(in int castlingIndex)
        {
            return castlingIndex switch
            {
                1 => Bitboard.F1G1,
                5 => Bitboard.B1C1D1,
                57 => Bitboard.F8G8,
                61 => Bitboard.B8C8D8,
                _ => throw new ArgumentException("Incorrect castling-index: " + castlingIndex)
            };
        }

        public static void UncastleRookUpdatePsqt(in ChessBoard cb, in int kingToIndex)
        {
            switch (kingToIndex)
            {
                case 1:
                    // white rook from 2 to 0
                    CastleRookUpdatePsqt(cb, 2, 0, White);
                    return;
                case 57:
                    // black rook from 58 to 56
                    CastleRookUpdatePsqt(cb, 58, 56, Black);
                    return;
                case 5:
                    // white rook from 4 to 7
                    CastleRookUpdatePsqt(cb, 4, 7, White);
                    return;
                case 61:
                    // black rook from 60 to 63
                    CastleRookUpdatePsqt(cb, 60, 63, Black);
                    return;
            }

            throw new ArgumentException("Incorrect king castling to-index: " + kingToIndex);
        }

        public static void CastleRookUpdateKeyAndPsqt(in ChessBoard cb, in int kingToIndex)
        {
            switch (kingToIndex)
            {
                case 1:
                    // white rook from 0 to 2
                    CastleRookUpdatePsqt(cb, 0, 2, White);
                    cb.ZobristKey ^= Zobrist.Piece[White][Rook][0] ^ Zobrist.Piece[White][Rook][2];
                    return;
                case 57:
                    // black rook from 56 to 58
                    CastleRookUpdatePsqt(cb, 56, 58, Black);
                    cb.ZobristKey ^= Zobrist.Piece[Black][Rook][56] ^ Zobrist.Piece[Black][Rook][58];
                    return;
                case 5:
                    // white rook from 7 to 4
                    CastleRookUpdatePsqt(cb, 7, 4, White);
                    cb.ZobristKey ^= Zobrist.Piece[White][Rook][7] ^ Zobrist.Piece[White][Rook][4];
                    return;
                case 61:
                    // black rook from 63 to 60
                    CastleRookUpdatePsqt(cb, 63, 60, Black);
                    cb.ZobristKey ^= Zobrist.Piece[Black][Rook][63] ^ Zobrist.Piece[Black][Rook][60];
                    return;
            }

            throw new ArgumentException("Incorrect king castling to-index: " + kingToIndex);
        }

        private static void CastleRookUpdatePsqt(in ChessBoard cb, in int fromIndex, in int toIndex, in int color)
        {
            cb.Pieces[color][All] ^= Util.PowerLookup[fromIndex] | Util.PowerLookup[toIndex];
            cb.Pieces[color][Rook] ^= Util.PowerLookup[fromIndex] | Util.PowerLookup[toIndex];
            cb.PieceIndexes[fromIndex] = Empty;
            cb.PieceIndexes[toIndex] = Rook;
            cb.PsqtScore += EvalConstants.Psqt[Rook][color][toIndex] - EvalConstants.Psqt[Rook][color][fromIndex];
        }

        public static bool IsValidCastlingMove(in ChessBoard cb, in int fromIndex, in int toIndex)
        {
            if (cb.CheckingPieces != 0)
            {
                return false;
            }

            if ((cb.AllPieces & GetRookInBetweenIndex(toIndex)) != 0)
            {
                return false;
            }

            var kingIndexes = InBetween[fromIndex][toIndex] | Util.PowerLookup[toIndex];
            while (kingIndexes != 0)
            {
                // king does not move through a checked position?
                if (CheckUtil.IsInCheckIncludingKing(BitOperations.TrailingZeroCount(kingIndexes), cb.ColorToMove,
                    cb.Pieces[cb.ColorToMoveInverse], cb.AllPieces))
                {
                    return false;
                }

                kingIndexes &= kingIndexes - 1;
            }

            return true;
        }
    }
}