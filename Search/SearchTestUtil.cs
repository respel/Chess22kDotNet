using System;
using Chess22kDotNet.Move;

namespace Chess22kDotNet.Search
{
    public static class SearchTestUtil
    {
        private const int Margin = 500;

        public static void TestTtValues(int score, int bestScore, int depth, int bestMove, int flag, long ttValue,
            int ply)
        {
            if (ttValue == 0 || TtUtil.GetDepth(ttValue) != depth) return;
            score = TtUtil.GetScore(ttValue, ply);
            if (TtUtil.GetFlag(ttValue) == TtUtil.FlagExact && flag == TtUtil.FlagExact)
            {
                if (score != bestScore)
                {
                    Console.WriteLine($"exact-exact: TT-score {score} != bestScore {bestScore}");
                }

                var move = TtUtil.GetMove(ttValue);
                if (move != bestMove)
                {
                    throw new ArgumentException(
                        $"Error: TT-move {new MoveWrapper(move)} != bestMove {new MoveWrapper(bestMove)}");
                }
            }
            else if (TtUtil.GetFlag(ttValue) == TtUtil.FlagLower && flag == TtUtil.FlagExact)
            {
                if (score - Margin > bestScore)
                {
                    Console.WriteLine($"lower-exact: TT-score {score} > bestScore {bestScore}");
                }
            }
            else if (TtUtil.GetFlag(ttValue) == TtUtil.FlagUpper && flag == TtUtil.FlagExact)
            {
                if (score + Margin < bestScore)
                {
                    Console.WriteLine($"upper-exact: TT-score {score} < bestScore {bestScore}");
                }
            }
            else if (TtUtil.GetFlag(ttValue) == TtUtil.FlagExact && flag == TtUtil.FlagLower)
            {
                if (score + Margin < bestScore)
                {
                    Console.WriteLine($"exact-lower: TT-score {score} < bestScore {bestScore}");
                }
            }
            else if (TtUtil.GetFlag(ttValue) == TtUtil.FlagExact && flag == TtUtil.FlagUpper)
            {
                if (score - Margin > bestScore)
                {
                    Console.WriteLine($"exact-upper: TT-score {score} > bestScore {bestScore}");
                }
            }
            else if (TtUtil.GetFlag(ttValue) == TtUtil.FlagUpper && flag == TtUtil.FlagLower)
            {
                if (score + Margin < bestScore)
                {
                    Console.WriteLine($"upper-lower: TT-score {score} < bestScore {bestScore}");
                }
            }
            else if (TtUtil.GetFlag(ttValue) == TtUtil.FlagLower && flag == TtUtil.FlagUpper)
            {
                if (score - Margin > bestScore)
                {
                    Console.WriteLine($"lower-upper: TT-score {score} > bestScore {bestScore}");
                }
            }
        }
    }
}