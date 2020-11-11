using System;
using System.Numerics;
using Chess22kDotNet.JavaWrappers;

namespace Chess22kDotNet.Texel
{
    public class TestSetStatistics
    {
        private static readonly int[] PieceCounts = new int[33];

        public static void Main()
        {
            var fens = Tuner.LoadFens("d:\\backup\\chess\\epds\\quiet-labeled.epd", true, false);
            Console.WriteLine(fens.Count + " fens found");

            var cb = ChessBoardInstances.Get(0);
            foreach (var entry in fens)
            {
                ChessBoardUtil.SetFen(entry.Key, cb);
                PieceCounts[BitOperations.PopCount((ulong) cb.AllPieces)]++;
            }

            for (var i = 0; i < 33; i++)
            {
                Console.WriteLine(i + " " + PieceCounts[i]);
            }
        }
    }
}