using System;
using System.Collections.Generic;
using System.Linq;
using Chess22kDotNet.Eval;
using Chess22kDotNet.Move;
using Chess22kDotNet.Search;

namespace Chess22kDotNet
{
    public static class Statistics
    {
        public const bool Enabled = false;

        public static long EvalNodes, AbNodes, SeeNodes;
        private static long _pvNodes;
        private static long _cutNodes;
        private static long _allNodes;
        public static long QNodes;
        public static long TtHits, TtMisses;
        public static int StaleMateCount, MateCount;
        public static long PawnEvalCacheHits, PawnEvalCacheMisses;
        public static long MaterialCacheMisses, MaterialCacheHits;
        private static int _bestMoveTt;
        private static int _bestMoveTtLower;
        private static int _bestMoveTtUpper;
        private static int _bestMoveCounter;
        private static int _bestMoveKiller1;
        private static int _bestMoveKiller2;
        private static int _bestMoveKillerEvasive1;
        private static int _bestMoveKillerEvasive2;
        private static int _bestMoveOther;
        private static int _bestMovePromotion;
        private static int _bestMoveWinningCapture;
        private static int _bestMoveLosingCapture;

        public static int Repetitions;
        private static int _repetitionTests;
        public static int CheckExtensions;
        public static int NullMoveHit, NullMoveMiss;
        public static long EvalCacheHits, EvalCacheMisses;
        public static readonly int[] Razored = new int[10];
        public static readonly int[] Futile = new int[10];
        public static readonly int[] StaticNullMoved = new int[10];
        public static readonly int[] Lmped = new int[10];
        public static readonly int[] FailHigh = new int[64];

        public static void Reset()
        {
            if (!Enabled) return;

            Array.Fill(Razored, 0);
            Array.Fill(Futile, 0);
            Array.Fill(StaticNullMoved, 0);
            Array.Fill(Lmped, 0);
            Array.Fill(FailHigh, 0);

            _bestMoveCounter = 0;
            QNodes = 0;
            _pvNodes = 1; // so we never divide by zero
            _cutNodes = 0;
            _allNodes = 0;
            PawnEvalCacheMisses = 0;
            PawnEvalCacheHits = 0;
            EvalNodes = 0;
            TtHits = 0;
            TtMisses = 0;
            StaleMateCount = 0;
            MateCount = 0;
            AbNodes = 0;
            SeeNodes = 0;
            Repetitions = 0;
            NullMoveHit = 0;
            NullMoveMiss = 0;
            _bestMoveTt = 0;
            _bestMoveTtLower = 0;
            _bestMoveTtUpper = 0;
            _bestMoveKiller1 = 0;
            _bestMoveKiller2 = 0;
            _bestMoveKillerEvasive1 = 0;
            _bestMoveKillerEvasive2 = 0;
            _bestMoveOther = 0;
            _bestMovePromotion = 0;
            _bestMoveWinningCapture = 0;
            _bestMoveLosingCapture = 0;
            CheckExtensions = 0;
            _repetitionTests = 0;
            EvalCacheHits = 0;
            EvalCacheMisses = 0;
        }

        public static void Print()
        {
            if (!Enabled) return;

            var totalMoveCount = ChessBoardUtil.CalculateTotalMoveCount();
            Console.WriteLine("AB-nodes      " + AbNodes);
            Console.WriteLine("PV-nodes      " + _pvNodes + " = 1/" + (_pvNodes + _cutNodes + _allNodes) / _pvNodes);
            Console.WriteLine("Cut-nodes     " + _cutNodes);
            PrintPercentage("Cut 1         ", FailHigh[0], _cutNodes - FailHigh[0]);
            PrintPercentage("Cut 2         ", FailHigh[1], _cutNodes - FailHigh[1]);
            PrintPercentage("Cut 3         ", FailHigh[2], _cutNodes - FailHigh[2]);
            Console.WriteLine("All-nodes     " + _allNodes);
            Console.WriteLine("Q-nodes       " + QNodes);
            Console.WriteLine("See-nodes     " + SeeNodes);
            Console.WriteLine("Evaluated     " + EvalNodes);
            Console.WriteLine("Moves         " + totalMoveCount);

            var threadData = ThreadData.GetInstance(0);
            Console.WriteLine("### Caches #######");
            PrintPercentage("TT            ", TtHits, TtMisses);
            Console.WriteLine("usage         " + TtUtil.GetUsagePercentage() / 10 + "%");
            PrintPercentage("Eval          ", EvalCacheHits, EvalCacheMisses);
            Console.WriteLine("usage         " + Util.GetUsagePercentage(threadData.EvalCache) + "%");
            PrintPercentage("Pawn eval     ", PawnEvalCacheHits, PawnEvalCacheMisses);
            Console.WriteLine("usage         " + Util.GetUsagePercentage(threadData.PawnCache) + "%");
            PrintPercentage("Material      ", MaterialCacheHits, MaterialCacheMisses);
            Console.WriteLine("usage         " + Util.GetUsagePercentage(threadData.MaterialCache) + "%");

            Console.WriteLine("## Best moves #####");
            Console.WriteLine("TT            " + _bestMoveTt);
            Console.WriteLine("TT-upper      " + _bestMoveTtUpper);
            Console.WriteLine("TT-lower      " + _bestMoveTtLower);
            Console.WriteLine("Win-cap       " + _bestMoveWinningCapture);
            Console.WriteLine("Los-cap       " + _bestMoveLosingCapture);
            Console.WriteLine("Promo         " + _bestMovePromotion);
            Console.WriteLine("Killer1       " + _bestMoveKiller1);
            Console.WriteLine("Killer2       " + _bestMoveKiller2);
            Console.WriteLine("Killer1 evasi " + _bestMoveKillerEvasive1);
            Console.WriteLine("Killer2 evasi " + _bestMoveKillerEvasive2);
            Console.WriteLine("Counter       " + _bestMoveCounter);
            Console.WriteLine("Other         " + _bestMoveOther);

            Console.WriteLine("### Outcome #####");
            Console.WriteLine("Checkmate     " + MateCount);
            Console.WriteLine("Stalemate     " + StaleMateCount);
            Console.WriteLine("Repetitions   " + Repetitions + "(" + _repetitionTests + ")");

            Console.WriteLine("### Extensions #####");
            Console.WriteLine("Check         " + CheckExtensions);

            Console.WriteLine("### Pruning #####");
            PrintPercentage("Null-move     ", NullMoveHit, NullMoveMiss);
            PrintDepthTotals("Static nmp    ", StaticNullMoved, false);
            PrintDepthTotals("Razored       ", Razored, false);
            PrintDepthTotals("Futile        ", Futile, false);
            PrintDepthTotals("LMP           ", Lmped, false);
        }

        private static void PrintDepthTotals(string message, IReadOnlyList<int> values, bool printDetails)
        {
            Console.WriteLine(message + values.Sum());
            if (!printDetails) return;
            for (var i = 0; i < values.Count; i++)
                if (values[i] != 0)
                    Console.WriteLine(i + " " + values[i]);
        }

        private static void PrintPercentage(string message, long hitCount, long failCount)
        {
            if (hitCount + failCount != 0)
                Console.WriteLine(message + hitCount + "/" + (failCount + hitCount) + " (" +
                                  hitCount * 100 / (hitCount + failCount) + "%)");
        }

        public static void SetBestMove(ChessBoard cb, int bestMove, int ttMove, TtEntry ttEntry, int flag,
            int counterMove,
            int killer1Move, int killer2Move)
        {
            if (!Enabled) return;

            switch (flag)
            {
                case TtUtil.FlagLower:
                    _cutNodes++;
                    break;
                case TtUtil.FlagUpper:
                    _allNodes++;
                    break;
                default:
                    _pvNodes++;
                    break;
            }

            if (bestMove == ttMove)
            {
                if (ttEntry.Flag == TtUtil.FlagLower)
                    _bestMoveTtLower++;
                else if (ttEntry.Flag == TtUtil.FlagUpper)
                    _bestMoveTtUpper++;
                else
                    _bestMoveTt++;
            }
            else if (MoveUtil.IsPromotion(bestMove))
            {
                _bestMovePromotion++;
            }
            else if (MoveUtil.GetAttackedPieceIndex(bestMove) != 0)
            {
                // slow but disabled when statistics are disabled
                if (SeeUtil.GetSeeCaptureScore(cb, bestMove) < 0)
                    _bestMoveLosingCapture++;
                else
                    _bestMoveWinningCapture++;
            }
            else if (bestMove == counterMove)
            {
                _bestMoveCounter++;
            }
            else if (bestMove == killer1Move && cb.CheckingPieces == 0)
            {
                _bestMoveKiller1++;
            }
            else if (bestMove == killer2Move && cb.CheckingPieces == 0)
            {
                _bestMoveKiller2++;
            }
            else if (bestMove == killer1Move && cb.CheckingPieces != 0)
            {
                _bestMoveKillerEvasive1++;
            }
            else if (bestMove == killer2Move && cb.CheckingPieces != 0)
            {
                _bestMoveKillerEvasive2++;
            }
            else
            {
                _bestMoveOther++;
            }
        }
    }
}