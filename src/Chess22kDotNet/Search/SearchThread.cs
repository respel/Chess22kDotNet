using System;
using Chess22kDotNet.Engine;

namespace Chess22kDotNet.Search
{
    public class SearchThread
    {
        // Laser based SMP skip
        private static readonly int[] SmpSkipDepths = {1, 1, 2, 2, 2, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4};
        private static readonly int[] SmpSkipAmount = {1, 2, 1, 2, 3, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 6};
        private static readonly int SmpMaxCycles = SmpSkipAmount.Length;

        private readonly int _threadNumber;
        private readonly ChessBoard _cb;
        private readonly ThreadData _threadData;

        public SearchThread(in int threadNumber)
        {
            _threadNumber = threadNumber;
            _cb = ChessBoardInstances.Get(threadNumber);
            _threadData = ThreadData.GetInstance(threadNumber);
        }

        public void Call()
        {
            try
            {
                //Thread.CurrentThread().setName("chess22k-search-" + threadNumber);
                if (_threadNumber == 0)
                {
                    RunMain();
                    NegamaxUtil.IsRunning = false;
                }
                else
                {
                    RunHelper();
                }
            }
            catch (Exception e)
            {
                ErrorLogger.Log(ChessBoardInstances.Get(_threadNumber), e, false);
            }
        }

        private void RunMain()
        {
            _threadData.ClearHistoryHeuristics();
            _threadData.InitPv(_cb);

            var depth = 0;
            var score = 0;
            var failLow = false;

            while (NegamaxUtil.IsRunning)
            {
                if (depth == MainEngine.MaxDepth)
                {
                    return;
                }

                depth++;

                var delta = EngineConstants.EnableAspiration && depth > 5 && Math.Abs(score) < 1000
                    ? EngineConstants.AspirationWindowDelta
                    : Util.ShortMax * 2;
                var alpha = Math.Max(score - delta, Util.ShortMin);
                var beta = Math.Min(score + delta, Util.ShortMax);

                while (NegamaxUtil.IsRunning)
                {
                    if (!TimeUtil.IsTimeLeft() && depth != 1 && !failLow)
                    {
                        return;
                    }

                    // System.out.println("start " + threadNumber + " " + depth);
                    score = NegamaxUtil.CalculateBestMove(_cb, _threadData, 0, depth, alpha, beta, 0);
                    // System.out.println("done " + threadNumber + " " + depth);

                    UciOut.SendPlyInfo(_threadData);
                    failLow = false;
                    if (score <= alpha)
                    {
                        failLow = true;
                        alpha = Math.Max(alpha - delta, Util.ShortMin);
                        delta *= 2;
                    }
                    else if (score >= beta)
                    {
                        beta = Math.Min(beta + delta, Util.ShortMax);
                        delta *= 2;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void RunHelper()
        {
            _threadData.ClearHistoryHeuristics();
            var cycleIndex = (_threadNumber - 1) % SmpMaxCycles;

            var depth = 0;
            var score = 0;

            while (depth < MainEngine.MaxDepth && NegamaxUtil.IsRunning)
            {
                depth++;
                if ((depth + cycleIndex) % SmpSkipDepths[cycleIndex] == 0)
                {
                    depth += SmpSkipAmount[cycleIndex];
                    if (depth > MainEngine.MaxDepth)
                    {
                        return;
                    }
                }

                var delta = EngineConstants.EnableAspiration && depth > 5 && Math.Abs(score) < 1000
                    ? EngineConstants.AspirationWindowDelta
                    : Util.ShortMax * 2;
                var alpha = Math.Max(score - delta, Util.ShortMin);
                var beta = Math.Min(score + delta, Util.ShortMax);

                while (NegamaxUtil.IsRunning)
                {
                    // System.out.println("start " + threadNumber + " " + depth);
                    score = NegamaxUtil.CalculateBestMove(_cb, _threadData, 0, depth, alpha, beta, 0);
                    // System.out.println("done " + threadNumber + " " + depth);

                    if (score <= alpha)
                    {
                        alpha = Math.Max(alpha - delta, Util.ShortMin);
                        delta *= 2;
                    }
                    else if (score >= beta)
                    {
                        beta = Math.Min(beta + delta, Util.ShortMax);
                        delta *= 2;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}