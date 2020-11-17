using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Chess22kDotNet.Eval;
using Chess22kDotNet.Move;
using Chess22kDotNet.Search;

namespace Chess22kDotNet.MainTests
{
    public class PerftMultiThreaded
    {
        private static readonly int[] Runs = {1, 1, 2, 4, 8, 16, 32};
        private const int Depth = 5;

        public static void Main()
        {
            ChessBoardInstances.Init(32);
            ThreadData.InitInstances(32);
            var stopwatch = new Stopwatch();

            foreach (var run in Runs)
            {
                stopwatch.Restart();
                Console.WriteLine("Starting " + run + " thread(s)");
                var tasks = new List<Task>();
                for (var i = 0; i < run; i++)
                {
                    var i1 = i;
                    tasks.Add(Task.Run(() => new PerftThread(i1).Run()));
                }

                Task.WaitAll(tasks.ToArray());
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }
        }

        private static int Perft(ChessBoard cb, ThreadData threadData, int depth)
        {
            threadData.StartPly();
            MoveGenerator.GenerateMoves(threadData, cb);
            MoveGenerator.GenerateAttacks(threadData, cb);

            if (depth == 0)
            {
                threadData.EndPly();
                return 1;
            }

            var counter = 0;
            while (threadData.HasNext())
            {
                var move = threadData.Next();
                if (!cb.IsLegal(move))
                {
                    continue;
                }

                cb.DoMove(move);
                EvalUtil.CalculateScore(cb, threadData);
                counter += Perft(cb, threadData, depth - 1);
                cb.UndoMove(move);
            }

            threadData.EndPly();
            return counter;
        }

        private class PerftThread
        {
            private readonly int _threadNumber;

            public PerftThread(int threadNumber)
            {
                _threadNumber = threadNumber;
            }

            public void Run()
            {
                var threadData = ThreadData.GetInstance(_threadNumber);
                var cb = ChessBoardInstances.Get(_threadNumber);
                ChessBoardUtil.SetFen(ChessConstants.FenStart, cb);
                Perft(cb, threadData, Depth);
            }
        }
    }
}