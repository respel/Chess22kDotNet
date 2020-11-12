using System;
using System.Collections.Generic;
using Chess22kDotNet.Eval;
using Chess22kDotNet.Search;

namespace Chess22kDotNet.Texel
{
    public class ErrorCalculator
    {
        private readonly Dictionary<string, double> _fens = new Dictionary<string, double>();
        private readonly ChessBoard _cb;
        private readonly ThreadData _threadData;

        public ErrorCalculator(ChessBoard cb, ThreadData threadData)
        {
            _cb = cb;
            _threadData = threadData;
        }

        public void AddFenWithScore(string fen, double score)
        {
            _fens.Add(fen, score);
        }

        public double Call()
        {
            double totalError = 0;

            foreach (var (key, value) in _fens)
            {
                ChessBoardUtil.SetFen(key, _cb);
                totalError += Math.Pow(
                    value - CalculateSigmoid(ChessConstants.ColorFactor[_cb.ColorToMove] *
                                             EvalUtil.CalculateScore(_cb, _threadData)),
                    2);
            }

            totalError /= _fens.Count;
            return totalError;
        }

        public static double CalculateSigmoid(int score)
        {
            return 1 / (1 + Math.Pow(10, -1.3 * score / 400));
        }
    }
}