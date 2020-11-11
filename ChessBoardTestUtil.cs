using System;
using System.Numerics;
using Chess22kDotNet.Eval;
using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet
{
    public static class ChessBoardTestUtil
    {
        public static void CompareScores(in ChessBoard cb)
        {
            var testCb = GetHorizontalMirroredCb(cb);
            CompareScores(cb, testCb, 1);

            testCb = GetVerticalMirroredCb(cb);
            CompareScores(cb, testCb, -1);
        }

        private static void CompareScores(in ChessBoard cb1, in ChessBoard cb2, in int factor)
        {
            EvalUtil.CalculateMobilityScoresAndSetAttacks(cb1);
            EvalUtil.CalculateMobilityScoresAndSetAttacks(cb2);

            if (KingSafetyEval.CalculateScores(cb2) != KingSafetyEval.CalculateScores(cb1) * factor)
            {
                Console.WriteLine("Unequal king-safety: " + KingSafetyEval.CalculateScores(cb1) + " " +
                                  KingSafetyEval.CalculateScores(cb2) * factor);
            }

            if (EvalUtil.CalculatePositionScores(cb1) != EvalUtil.CalculatePositionScores(cb2) * factor)
            {
                Console.WriteLine("Unequal position score: " + EvalUtil.CalculatePositionScores(cb1) + " " +
                                  EvalUtil.CalculatePositionScores(cb2) * factor);
            }

            // if (EvalUtil.getPawnScores(cb1) != EvalUtil.getPawnScores(cb2) * factor) {
            // System.out.println("Unequal pawns: " + EvalUtil.getPawnScores(cb1) + " " + EvalUtil.getPawnScores(cb2) *
            // factor);
            // }
            // if (EvalUtil.getImbalances(cb1) != EvalUtil.getImbalances(cb2) * factor) {
            // System.out.println("Unequal imbalances: " + EvalUtil.getImbalances(cb1) + " " + EvalUtil.getImbalances(cb2) *
            // factor);
            // }
            if (EvalUtil.CalculateOthers(cb2) != EvalUtil.CalculateOthers(cb1) * factor)
            {
                Console.WriteLine("Unequal others: " + EvalUtil.CalculateOthers(cb1) + " " +
                                  EvalUtil.CalculateOthers(cb2) * factor);
            }

            if (EvalUtil.CalculateThreats(cb2) != EvalUtil.CalculateThreats(cb1) * factor)
            {
                Console.WriteLine("Unequal threats: " + EvalUtil.CalculateThreats(cb1) + " " +
                                  EvalUtil.CalculateThreats(cb2) * factor);
            }

            if (PassedPawnEval.CalculateScores(cb1) != PassedPawnEval.CalculateScores(cb2) * factor)
            {
                Console.WriteLine("Unequal passed-pawns: " + PassedPawnEval.CalculateScores(cb1) + " " +
                                  PassedPawnEval.CalculateScores(cb2) * factor);
            }
        }

        public static void TestValues(ChessBoard cb)
        {
            var iterativeZk = cb.ZobristKey;
            var iterativeZkPawn = cb.PawnZobristKey;
            var iterativeAllPieces = cb.AllPieces;
            var iterativePsqt = cb.PsqtScore;
            var phase = cb.Phase;
            long materialKey = cb.MaterialKey;
            var testPieceIndexes = new int[64];
            Array.Copy(cb.PieceIndexes, testPieceIndexes, cb.PieceIndexes.Length);

            Assert.IsTrue(BitOperations.TrailingZeroCount(cb.Pieces[White][King]) == cb.KingIndex[White]);
            Assert.IsTrue(BitOperations.TrailingZeroCount(cb.Pieces[Black][King]) == cb.KingIndex[Black]);

            ChessBoardUtil.Init(cb);

            // zobrist keys
            Assert.IsTrue(iterativeZk == cb.ZobristKey);
            Assert.IsTrue(iterativeZkPawn == cb.PawnZobristKey);

            // combined pieces
            Assert.IsTrue(iterativeAllPieces == cb.AllPieces);

            // psqt
            Assert.IsTrue(iterativePsqt == cb.PsqtScore);

            // piece-indexes
            for (var i = 0; i < testPieceIndexes.Length; i++)
            {
                Assert.IsTrue(testPieceIndexes[i] == cb.PieceIndexes[i]);
            }

            Assert.IsTrue(phase == cb.Phase);
            Assert.IsTrue(materialKey == cb.MaterialKey);
        }

        private static ChessBoard GetHorizontalMirroredCb(ChessBoard cb)
        {
            var testCb = ChessBoardInstances.Get(1);

            for (var color = White; color <= Black; color++)
            {
                for (var piece = Pawn; piece <= King; piece++)
                {
                    testCb.Pieces[color][piece] = Util.MirrorHorizontal(cb.Pieces[color][piece]);
                }
            }

            testCb.ColorToMove = cb.ColorToMove;
            ChessBoardUtil.Init(testCb);
            testCb.MoveCounter = cb.MoveCounter;
            return testCb;
        }

        private static ChessBoard GetVerticalMirroredCb(ChessBoard cb)
        {
            var testCb = ChessBoardInstances.Get(1);

            for (var piece = Pawn; piece <= King; piece++)
            {
                testCb.Pieces[White][piece] = Util.MirrorVertical(cb.Pieces[Black][piece]);
            }

            for (var piece = Pawn; piece <= King; piece++)
            {
                testCb.Pieces[Black][piece] = Util.MirrorVertical(cb.Pieces[White][piece]);
            }

            testCb.ColorToMove = cb.ColorToMoveInverse;
            ChessBoardUtil.Init(testCb);
            testCb.MoveCounter = cb.MoveCounter;
            return testCb;
        }
    }
}