using System;
using Chess22kDotNet.Move;

namespace Chess22kDotNet.Search
{
    public static class SearchTestUtil
    {
        private const int Margin = 500;

        public static void TestTtValues(int score, int bestScore, int depth, int bestMove, int flag, TtEntry ttEntry,
            int ply)
        {
            if (ttEntry.Key == 0 || ttEntry.Depth != depth) return;
            score = ttEntry.GetScore(ply);
            if (ttEntry.Flag == TtUtil.FlagExact && flag == TtUtil.FlagExact)
            {
                if (score != bestScore)
                {
                    Console.WriteLine($"exact-exact: TT-score {score} != bestScore {bestScore}");
                }

                var move = ttEntry.Move;
                if (move != bestMove)
                {
                    throw new ArgumentException(
                        $"Error: TT-move {new MoveWrapper(move)} != bestMove {new MoveWrapper(bestMove)}");
                }
            }
            else if (ttEntry.Flag == TtUtil.FlagLower && flag == TtUtil.FlagExact)
            {
                if (score - Margin > bestScore)
                {
                    Console.WriteLine($"lower-exact: TT-score {score} > bestScore {bestScore}");
                }
            }
            else if (ttEntry.Flag == TtUtil.FlagUpper && flag == TtUtil.FlagExact)
            {
                if (score + Margin < bestScore)
                {
                    Console.WriteLine($"upper-exact: TT-score {score} < bestScore {bestScore}");
                }
            }
            else if (ttEntry.Flag == TtUtil.FlagExact && flag == TtUtil.FlagLower)
            {
                if (score + Margin < bestScore)
                {
                    Console.WriteLine($"exact-lower: TT-score {score} < bestScore {bestScore}");
                }
            }
            else if (ttEntry.Flag == TtUtil.FlagExact && flag == TtUtil.FlagUpper)
            {
                if (score - Margin > bestScore)
                {
                    Console.WriteLine($"exact-upper: TT-score {score} > bestScore {bestScore}");
                }
            }
            else if (ttEntry.Flag == TtUtil.FlagUpper && flag == TtUtil.FlagLower)
            {
                if (score + Margin < bestScore)
                {
                    Console.WriteLine($"upper-lower: TT-score {score} < bestScore {bestScore}");
                }
            }
            else if (ttEntry.Flag == TtUtil.FlagLower && flag == TtUtil.FlagUpper)
            {
                if (score - Margin > bestScore)
                {
                    Console.WriteLine($"lower-upper: TT-score {score} > bestScore {bestScore}");
                }
            }
        }
    }
}