using System;
using System.Diagnostics;
using System.Reflection;
using Chess22kDotNet.Eval;
using Chess22kDotNet.Move;
using Chess22kDotNet.Search;

namespace Chess22kDotNet.Engine
{
    public static class UciOut
    {
        public static bool NoOutput = false;
        private static readonly Stopwatch Stopwatch = new Stopwatch();
        
        public static void SendUci()
        {
            Console.WriteLine("id name Chess22kDotNet " + GetVersion());
            Console.WriteLine("id author respel");
            Console.WriteLine("option name Hash type spin default 128 min 1 max 16384");
            Console.WriteLine("option name Threads type spin default 1 min 1 max " + EngineConstants.MaxThreads);
            Console.WriteLine("option name Ponder type check default false");
            Console.WriteLine("uciok");
        }

        public static void SendBestMove(ThreadData threadData)
        {
            if (NoOutput)
            {
                return;
            }

            Statistics.Print();
            if (UciOptions.Ponder && threadData.GetPonderMove() != 0)
            {
                Console.WriteLine("bestmove " + new MoveWrapper(threadData.GetBestMove()) + " ponder " +
                                  new MoveWrapper(threadData.GetPonderMove()));
            }
            else
            {
                Console.WriteLine("bestmove " + new MoveWrapper(threadData.GetBestMove()));
            }
        }

        private static long CalculateNps(long totalMoveCount)
        {
            return totalMoveCount * 1000 / Math.Max(TimeUtil.GetPassedTimeMs(), 1);
        }

        public static void SendInfo()
        {
            if (NoOutput)
            {
                return;
            }

            if (Stopwatch.IsRunning && Stopwatch.ElapsedMilliseconds < 2000)
            {
                return;
            }

            var totalMoveCount = ChessBoardUtil.CalculateTotalMoveCount();
            Console.WriteLine("info nodes " + totalMoveCount + " nps " + CalculateNps(totalMoveCount) + " hashfull " +
                              TtUtil.GetUsagePercentage());
        }

        public static void SendPlyInfo(ThreadData threadData)
        {
            if (NoOutput)
            {
                return;
            }

            Stopwatch.Restart();

            var totalMoveCount = ChessBoardUtil.CalculateTotalMoveCount();

            // info depth 1 seldepth 2 score cp 50 pv d2d4 d7d5 e2e3 hashfull 0 nps 1000 nodes 22
            // info depth 4 seldepth 10 score cp 40 upperbound pv d2d4 d7d5 e2e3 hashfull 0 nps 30000 nodes 1422
            Console.WriteLine("info depth " + threadData.Depth + " time " + TimeUtil.GetPassedTimeMs() + " score cp " +
                              threadData.BestScore + threadData.ScoreType.ToFriendlyName()
                              + "nps " + CalculateNps(totalMoveCount) + " nodes " + totalMoveCount + " hashfull " +
                              TtUtil.GetUsagePercentage() + " pv "
                              + PvUtil.AsString(threadData.Pv));
        }

        public static void Eval(ChessBoard cb, ThreadData threadData)
        {
            var mobilityScore = EvalUtil.CalculateMobilityScoresAndSetAttacks(cb);
            Console.WriteLine(" Material imbalance: " + EvalUtil.GetImbalances(cb, threadData.MaterialCache));
            Console.WriteLine("          Position : " + GetMgEgString(cb.PsqtScore));
            Console.WriteLine("          Mobility : " + GetMgEgString(mobilityScore));
            Console.WriteLine(" Pawn : " + EvalUtil.GetPawnScores(cb, threadData.PawnCache));
            Console.WriteLine("       Pawn-passed : " + GetMgEgString(PassedPawnEval.CalculateScores(cb)));
            Console.WriteLine("       Pawn shield : " + GetMgEgString(EvalUtil.CalculatePawnShieldBonus(cb)));
            Console.WriteLine("       King-safety : " + KingSafetyEval.CalculateScores(cb));
            Console.WriteLine("           Threats : " + GetMgEgString(EvalUtil.CalculateThreats(cb)));
            Console.WriteLine("             Other : " + EvalUtil.CalculateOthers(cb));
            Console.WriteLine("             Space : " + EvalUtil.CalculateSpace(cb));
            Console.WriteLine("-----------------------------");
            Console.WriteLine(" Total : " +
                              ChessConstants.ColorFactor[cb.ColorToMove] * EvalUtil.GetScore(cb, threadData));
        }

        private static string GetMgEgString(int mgEgScore)
        {
            return EvalUtil.GetMgScore(mgEgScore) + "/" + EvalUtil.GetEgScore(mgEgScore);
        }

        public static string GetVersion()
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return assemblyVersion;
        }
    }
}