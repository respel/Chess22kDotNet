using System;
using Chess22kDotNet.Eval;
using Chess22kDotNet.Search;

namespace Chess22kDotNet.Texel
{
    public class LargestErrorCalculator
    {
        private static readonly double[] LargestError = new double[100];
        private static readonly string[] LargestErrorFen = new string[100];

        public static void Main()
        {
            var fens = Tuner.LoadFens("d:\\backup\\chess\\epds\\quiet-labeled.epd", true, false);
            Console.WriteLine(fens.Count + " fens found");

            var cb = ChessBoardInstances.Get(0);
            var threadData = ThreadData.GetInstance(0);
            foreach (var (key, value) in fens)
            {
                ChessBoardUtil.SetFen(key, cb);
                var error = Math.Pow(value - ErrorCalculator.CalculateSigmoid(EvalUtil.CalculateScore(cb, threadData)),
                    2);

                for (var i = 0; i < LargestError.Length; i++)
                {
                    if (!(error > LargestError[i])) continue;
                    LargestError[i] = error;
                    LargestErrorFen[i] = key;
                    break;
                }
            }

            for (var i = 0; i < LargestError.Length; i++)
            {
                Console.WriteLine($"{LargestErrorFen[i],60} -> {LargestError[i]}");
            }
        }
    }
}