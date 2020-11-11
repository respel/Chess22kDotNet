using System;
using System.Collections.Generic;
using System.Numerics;

namespace Chess22kDotNet
{
    public static class Util
    {
        public const int ShortMin = -32767;
        public const int ShortMax = 32767;

        private static readonly byte[][] Distance = CreateJaggedArray<byte[][]>(64, 64);
        public static readonly long[] PowerLookup = new long[64];

        static Util()
        {
            for (var i = 0; i < 64; i++)
            {
                for (var j = 0; j < 64; j++)
                {
                    Distance[i][j] = (byte) Math.Max(Math.Abs((i >> 3) - (j >> 3)), Math.Abs((i & 7) - (j & 7)));
                }
            }

            for (var i = 0; i < 64; i++)
            {
                PowerLookup[i] = 1L << i;
            }
        }

        public static string ToFriendlyName(this ChessConstants.ScoreType rating)
        {
            return rating switch
            {
                ChessConstants.ScoreType.Exact => " ",
                ChessConstants.ScoreType.Lower => " lowerbound ",
                ChessConstants.ScoreType.Upper => " upperbound ",
                _ => throw new ArgumentException("Invalid ScoreType")
            };
        }

        public static long RightTripleShift(long a, int shiftAmount)
        {
            return (long) ((ulong) a >> shiftAmount);
        }

        public static int RightTripleShift(int a, int shiftAmount)
        {
            return (int) ((uint) a >> shiftAmount);
        }

        public static T CreateJaggedArray<T>(params int[] lengths)
        {
            return (T) InitializeJaggedArray(typeof(T).GetElementType(), 0, lengths);
        }

        private static object InitializeJaggedArray(Type type, int index, IReadOnlyList<int> lengths)
        {
            var array = Array.CreateInstance(type, lengths[index]);
            var elementType = type.GetElementType();

            if (elementType == null) return array;
            for (var i = 0; i < lengths[index]; i++)
            {
                array.SetValue(
                    InitializeJaggedArray(elementType, index + 1, lengths), i);
            }

            return array;
        }


        private static long ReverseBytes(long i)
        {
            i = (i & 0x00ff00ff00ff00ffL) << 8 | RightTripleShift(i, 8) & 0x00ff00ff00ff00ffL;
            return (i << 48) | ((i & 0xffff0000L) << 16) |
                   (RightTripleShift(i, 16) & 0xffff0000L) | RightTripleShift(i, 48);
        }

        public static void Reverse(int[] array)
        {
            for (var i = 0; i < array.Length / 2; i++)
            {
                var temp = array[i];
                array[i] = array[array.Length - 1 - i];
                array[array.Length - 1 - i] = temp;
            }
        }

        public static void Reverse(long[] array)
        {
            for (var i = 0; i < array.Length / 2; i++)
            {
                var temp = array[i];
                array[i] = array[array.Length - 1 - i];
                array[array.Length - 1 - i] = temp;
            }
        }

        public static long MirrorHorizontal(long bitboard)
        {
            const long k1 = 0x5555555555555555L;
            const long k2 = 0x3333333333333333L;
            const long k4 = 0x0f0f0f0f0f0f0f0fL;
            bitboard = (RightTripleShift(bitboard, 1) & k1) | ((bitboard & k1) << 1);
            bitboard = (RightTripleShift(bitboard, 2) & k2) | ((bitboard & k2) << 2);
            bitboard = (RightTripleShift(bitboard, 4) & k4) | ((bitboard & k4) << 4);
            return bitboard;
        }

        public static int FlipHorizontalIndex(int index)
        {
            return (index & 0xF8) | (7 - (index & 7));
        }

        public static long MirrorVertical(long bitboard)
        {
            return ReverseBytes(bitboard);
        }

        public static int GetDistance(in int index1, in int index2)
        {
            return Distance[index1][index2];
        }

        public static int GetDistance(in long sq1, in long sq2)
        {
            return GetDistance(BitOperations.TrailingZeroCount(sq1), BitOperations.TrailingZeroCount(sq2));
        }

        public static int GetUsagePercentage(long[] keys)
        {
            var usage = 0;
            for (var i = 0; i < 1000; i++)
            {
                if (keys[i] != 0)
                {
                    usage++;
                }
            }

            return usage / 10;
        }

        public static int GetUsagePercentage(int[] keys)
        {
            var usage = 0;
            for (var i = 0; i < 1000; i++)
            {
                if (keys[i] != 0)
                {
                    usage++;
                }
            }

            return usage / 10;
        }

        /**
	    * returns the black corresponding square
	    */
        public static int GetRelativeSquare(in int color, in int index)
        {
            return index ^ (56 * color);
        }
    }
}