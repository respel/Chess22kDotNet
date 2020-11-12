using System;
using Chess22kDotNet.Eval;
using Chess22kDotNet.Move;
using Chess22kDotNet.Search;
using Chess22kDotNet.Texel;

namespace Chess22kDotNet.MainTests
{
    /**
    * compares SEE scores vs Quiescence scores (material score with attacks on the same square)
    *
    */
    public class SeeTest
    {
        private static ThreadData _threadData = new ThreadData(0);

        public static void Main()
        {
            var cb = ChessBoardInstances.Get(0);

            // read all fens, including score
            var fens = Tuner.LoadFens("d:\\backup\\chess\\epds\\violent.epd", false, true);
            Console.WriteLine("Fens found : " + fens.Count);

            double sameScore = 0;
            double totalAttacks = 0;
            var start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            foreach (var entry in fens)
            {
                ChessBoardUtil.SetFen(entry.Key, cb);
                _threadData.StartPly();
                MoveGenerator.GenerateAttacks(_threadData, cb);
                while (_threadData.HasNext())
                {
                    var move = _threadData.Next();
                    if (!cb.IsLegal(move))
                    {
                        continue;
                    }

                    totalAttacks++;
                    var seeScore = SeeUtil.GetSeeCaptureScore(cb, move);
                    var materialScore = EvalUtil.CalculateMaterialScore(cb);
                    var qScore = ChessConstants.ColorFactor[cb.ColorToMoveInverse] * materialScore -
                                 CalculateQScore(cb, move, true);
                    if (seeScore == qScore)
                    {
                        sameScore++;
                    }

                    // else {
                    // seeScore = SEEUtil.getSeeCaptureScore(cb, move);
                    // qScore = ChessConstants.COLOR_FACTOR[cb.colorToMoveInverse] * materialScore - calculateQScore(cb,
                    // move, true);
                    // }
                }

                _threadData.EndPly();
            }

            Console.WriteLine($"{sameScore:f0} {totalAttacks:f0} = {sameScore / totalAttacks:f4}");
            Console.WriteLine("msec: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start));
        }

        private static int CalculateQScore(ChessBoard cb, int move, bool isFirstMove)
        {
            var bestScore = Util.ShortMin;

            cb.DoMove(move);

            _threadData.StartPly();
            MoveGenerator.GenerateAttacks(_threadData, cb);

            var movePerformed = false;
            while (_threadData.HasNext())
            {
                // only attacks on the same square
                var currentMove = _threadData.Next();
                if (!cb.IsLegal(currentMove))
                {
                    continue;
                }

                if (MoveUtil.GetToIndex(currentMove) != MoveUtil.GetToIndex(move))
                {
                    continue;
                }

                var score = -CalculateQScore(cb, currentMove, false);
                score = Math.Max(score,
                    ChessConstants.ColorFactor[cb.ColorToMove] * EvalUtil.CalculateMaterialScore(cb));

                movePerformed = true;
                if (score > bestScore)
                {
                    bestScore = score;
                }
            }

            _threadData.EndPly();

            if (!movePerformed)
            {
                bestScore = ChessConstants.ColorFactor[cb.ColorToMove] * EvalUtil.CalculateMaterialScore(cb);
            }

            cb.UndoMove(move);
            return bestScore;
        }
    }
}