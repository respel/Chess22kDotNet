using System;
using System.Numerics;
using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet
{
    public static class Zobrist
    {
        public static readonly long SideToMove;
        public static readonly long[] Castling = new long[16];
        public static readonly long[] EpIndex = new long[48];
        public static readonly long[][][] Piece = Util.CreateJaggedArray<long[][][]>(2, 7, 64);

        static Zobrist()
        {
            var r = new Random();
            for (var colorIndex = 0; colorIndex <= Black; colorIndex++)
                for (var pieceIndex = 0; pieceIndex <= King; pieceIndex++)
                    for (var square = 0; square < 64; square++)
                        Piece[colorIndex][pieceIndex][square] = LongRandom(r);

            for (var i = 0; i < Castling.Length; i++) Castling[i] = LongRandom(r);

            // skip first item: contains only zeros, default value and has no effect when xoring
            for (var i = 1; i < EpIndex.Length; i++) EpIndex[i] = LongRandom(r);

            SideToMove = LongRandom(r);
        }

        private static long LongRandom(Random rand)
        {
            var buf = new byte[8];
            rand.NextBytes(buf);
            return BitConverter.ToInt64(buf, 0);
        }

        public static void SetKey(ChessBoard cb)
        {
            cb.ZobristKey = 0;

            for (var color = 0; color < 2; color++)
                for (var pieceType = Pawn; pieceType <= King; pieceType++)
                {
                    var pieces = cb.Pieces[color][pieceType];
                    while (pieces != 0)
                    {
                        cb.ZobristKey ^= Piece[color][pieceType][BitOperations.TrailingZeroCount(pieces)];
                        pieces &= pieces - 1;
                    }
                }

            cb.ZobristKey ^= Castling[cb.CastlingRights];
            if (cb.ColorToMove == White) cb.ZobristKey ^= SideToMove;

            cb.ZobristKey ^= EpIndex[cb.EpIndex];
        }

        public static void SetPawnKey(ChessBoard cb)
        {
            cb.PawnZobristKey = 0;

            var pieces = cb.Pieces[White][Pawn];
            while (pieces != 0)
            {
                cb.PawnZobristKey ^= Piece[White][Pawn][BitOperations.TrailingZeroCount(pieces)];
                pieces &= pieces - 1;
            }

            pieces = cb.Pieces[Black][Pawn];
            while (pieces != 0)
            {
                cb.PawnZobristKey ^= Piece[Black][Pawn][BitOperations.TrailingZeroCount(pieces)];
                pieces &= pieces - 1;
            }
        }
    }
}