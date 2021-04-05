using System;
using System.Diagnostics;
using Chess22kDotNet.Engine;
using Chess22kDotNet.Search;
using Chess22kDotNet.Texel;

namespace Chess22kDotNet.MainTests
{
    /**
    * * compares regular search scores vs Quiescence scores
    *
    */
    public class QSearchTest
    {
        private static readonly ThreadData ThreadData = new ThreadData(0);

        public static void Main()
        {
            var cb = ChessBoardInstances.Get(0);

            // read all fens, including score
            var fens = Tuner.LoadFens("d:\\backup\\chess\\epds\\violent.epd", false, true);
            Console.WriteLine("Fens found : " + fens.Count);

            // NegamaxUtil.isRunning = true;
            EngineConstants.Power2TtEntries = 1;
            TtUtil.Init(false);

            double totalPositions = 0;
            double sameScore = 0;
            long totalError = 0;
            var watch = new Stopwatch();
            watch.Start();
            foreach (var entry in fens)
            {
                ChessBoardUtil.SetFen(entry.Key, cb);
                if (cb.CheckingPieces == 0) continue;

                totalPositions++;
                var searchScore = NegamaxUtil.CalculateBestMove(cb, ThreadData, 0, 1, Util.ShortMin, Util.ShortMax, 0);
                TtUtil.ClearValues();
                var qScore = QuiescenceUtil.CalculateBestMove(cb, ThreadData, Util.ShortMin, Util.ShortMax);

                if (searchScore == qScore)
                {
                    sameScore++;
                }
                else
                {
                    var error = searchScore - qScore;
                    // if (error > 500) {
                    // System.out.println(searchScore + " " + qScore);
                    // QuiescenceUtil.calculateBestMove(cb, threadData, Util.SHORT_MIN, Util.SHORT_MAX);
                    // }

                    totalError += error;
                }
            }

            var averageError = (int)(totalError / (totalPositions - sameScore));
            Console.WriteLine($"{sameScore / totalPositions:f4} {averageError}");
            Console.WriteLine("msec: " + watch.ElapsedMilliseconds);
        }
    }
}