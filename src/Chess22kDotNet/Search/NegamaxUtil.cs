using System;
using Chess22kDotNet.Engine;
using Chess22kDotNet.Eval;
using Chess22kDotNet.Move;

namespace Chess22kDotNet.Search
{
    public static class NegamaxUtil
    {
        private const int PhaseTt = 0;
        private const int PhaseAttacking = 1;
        private const int PhaseKiller1 = 2;
        private const int PhaseKiller2 = 3;
        private const int PhaseCounter = 4;
        private const int PhaseQuiet = 5;

        // Margins shamelessly stolen from Laser
        private static readonly int[] StaticNullmoveMargin = {0, 60, 130, 210, 300, 400, 510};
        private static readonly int[] RazoringMargin = {0, 240, 280, 300};
        private static readonly int[] FutilityMargin = {0, 80, 170, 270, 380, 500, 630};
        private static readonly int[][] LmrTable = Util.CreateJaggedArray<int[][]>(64, 64);

        static NegamaxUtil()
        {
            // Ethereal LMR formula with depth and number of performed moves
            for (var depth = 1; depth < 64; depth++)
            {
                for (var moveNumber = 1; moveNumber < 64; moveNumber++)
                {
                    LmrTable[depth][moveNumber] = (int) (0.6f + Math.Log(depth) * Math.Log(moveNumber * 1.2f) / 2.5f);
                }
            }
        }

        public static bool IsRunning = false;

        public static int CalculateBestMove(ChessBoard cb, ThreadData threadData, int ply, int depth,
            int alpha, int beta,
            int nullMoveCounter)
        {
            if (!IsRunning)
            {
                return ChessConstants.ScoreNotRunning;
            }

            if (EngineConstants.Assert)
            {
                Assert.IsTrue(depth >= 0);
                Assert.IsTrue(alpha >= Util.ShortMin && alpha <= Util.ShortMax);
                Assert.IsTrue(beta >= Util.ShortMin && beta <= Util.ShortMax);
            }

            var alphaOrig = alpha;

            // get extensions
            depth += Extensions(cb);

            /* mate-distance pruning */
            if (EngineConstants.EnableMateDistancePruning)
            {
                alpha = Math.Max(alpha, Util.ShortMin + ply);
                beta = Math.Min(beta, Util.ShortMax - ply - 1);
                if (alpha >= beta)
                {
                    return alpha;
                }
            }

            // TODO JITWatch unpredictable branch
            if (depth == 0)
            {
                return QuiescenceUtil.CalculateBestMove(cb, threadData, alpha, beta);
            }

            /* transposition-table */
            var ttEntry = TtUtil.GetEntry(cb.ZobristKey);
            var score = TtUtil.GetScore(ttEntry, ply);
            if (ttEntry.Key != 0)
            {
                if (!EngineConstants.TestTtValues)
                {
                    if (ttEntry.Depth >= depth)
                    {
                        switch (ttEntry.Flag)
                        {
                            case TtUtil.FlagExact:
                                return score;
                            case TtUtil.FlagLower:
                                if (score >= beta)
                                {
                                    return score;
                                }

                                break;
                            case TtUtil.FlagUpper:
                                if (score <= alpha)
                                {
                                    return score;
                                }

                                break;
                        }
                    }
                }
            }

            if (Statistics.Enabled)
            {
                Statistics.AbNodes++;
            }

            var eval = Util.ShortMin;
            var isPv = beta - alpha != 1;
            if (!isPv && cb.CheckingPieces == 0)
            {
                eval = EvalUtil.GetScore(cb, threadData);

                /* use tt value as eval */
                if (EngineConstants.UseTtScoreAsEval)
                {
                    if (TtUtil.CanRefineEval(ttEntry, eval, score))
                    {
                        eval = score;
                    }
                }

                /* static null move pruning */
                if (EngineConstants.EnableStaticNullMove && depth < StaticNullmoveMargin.Length)
                {
                    if (eval - StaticNullmoveMargin[depth] >= beta)
                    {
                        if (Statistics.Enabled)
                        {
                            Statistics.StaticNullMoved[depth]++;
                        }

                        return eval;
                    }
                }

                /* razoring */
                if (EngineConstants.EnableRazoring && depth < RazoringMargin.Length &&
                    Math.Abs(alpha) < EvalConstants.ScoreMateBound)
                {
                    if (eval + RazoringMargin[depth] < alpha)
                    {
                        score = QuiescenceUtil.CalculateBestMove(cb, threadData, alpha - RazoringMargin[depth],
                            alpha - RazoringMargin[depth] + 1);
                        if (score + RazoringMargin[depth] <= alpha)
                        {
                            if (Statistics.Enabled)
                            {
                                Statistics.Razored[depth]++;
                            }

                            return score;
                        }
                    }
                }

                /* null-move */
                if (EngineConstants.EnableNullMove)
                {
                    if (nullMoveCounter < 2 && eval >= beta &&
                        MaterialUtil.HasNonPawnPieces(cb.MaterialKey, cb.ColorToMove))
                    {
                        cb.DoNullMove();
                        // TODO less reduction if stm (other side) has only 1 major piece
                        var reduction = depth / 4 + 3 + Math.Min((eval - beta) / 80, 3);
                        score = depth - reduction <= 0
                            ? -QuiescenceUtil.CalculateBestMove(cb, threadData, -beta, -beta + 1)
                            : -CalculateBestMove(cb, threadData, ply + 1, depth - reduction, -beta, -beta + 1,
                                nullMoveCounter + 1);
                        cb.UndoNullMove();
                        if (score >= beta)
                        {
                            if (Statistics.Enabled)
                            {
                                Statistics.NullMoveHit++;
                            }

                            return score;
                        }

                        if (Statistics.Enabled)
                        {
                            Statistics.NullMoveMiss++;
                        }
                    }
                }
            }

            var parentMove = ply == 0 ? 0 : threadData.Previous();
            var bestMove = 0;
            var bestScore = Util.ShortMin - 1;
            var ttMove = 0;
            var counterMove = 0;
            var killer1Move = 0;
            var killer2Move = 0;
            var movesPerformed = 0;

            threadData.StartPly();
            var phase = PhaseTt;
            while (phase <= PhaseQuiet)
            {
                switch (phase)
                {
                    case PhaseTt:
                        if (ttEntry.Key != 0)
                        {
                            ttMove = ttEntry.Move;
                            if (cb.IsValidMove(ttMove))
                            {
                                threadData.AddMove(ttMove);
                            }

                            // else {
                            // throw new RuntimeException("invalid tt-move found: " + new MoveWrapper(ttMove));
                            // }
                        }

                        break;
                    case PhaseAttacking:
                        MoveGenerator.GenerateAttacks(threadData, cb);
                        threadData.SetMvvlvaScores();
                        threadData.Sort();
                        break;
                    case PhaseKiller1:
                        killer1Move = threadData.GetKiller1(ply);
                        if (killer1Move != 0 && killer1Move != ttMove && cb.IsValidMove(killer1Move))
                        {
                            threadData.AddMove(killer1Move);
                            break;
                        }

                        phase++;
                        goto case PhaseKiller2;
                    case PhaseKiller2:
                        killer2Move = threadData.GetKiller2(ply);
                        if (killer2Move != 0 && killer2Move != ttMove && cb.IsValidMove(killer2Move))
                        {
                            threadData.AddMove(killer2Move);
                            break;
                        }

                        phase++;
                        goto case PhaseCounter;
                    case PhaseCounter:
                        counterMove = threadData.GetCounter(cb.ColorToMove, parentMove);
                        if (counterMove != 0 && counterMove != ttMove && counterMove != killer1Move &&
                            counterMove != killer2Move && cb.IsValidMove(counterMove))
                        {
                            threadData.AddMove(counterMove);
                            break;
                        }

                        phase++;
                        goto case PhaseQuiet;
                    case PhaseQuiet:
                        MoveGenerator.GenerateMoves(threadData, cb);
                        threadData.SetHhScores(cb.ColorToMove);
                        threadData.Sort();
                        break;
                }

                while (threadData.HasNext())
                {
                    var move = threadData.Next();

                    switch (phase)
                    {
                        case PhaseQuiet when move == ttMove || move == killer1Move || move == killer2Move ||
                                             move == counterMove ||
                                             !cb.IsLegal(move):
                        case PhaseAttacking when move == ttMove || !cb.IsLegal(move):
                            continue;
                    }

                    // pruning allowed?
                    if (!isPv && cb.CheckingPieces == 0 && movesPerformed > 0 && threadData.GetMoveScore() < 100
                        && !cb.IsDiscoveredMove(MoveUtil.GetFromIndex(move)))
                    {
                        if (phase == PhaseQuiet)
                        {
                            /* late move pruning */
                            if (EngineConstants.EnableLmp && depth <= 4 && movesPerformed >= depth * 3 + 3)
                            {
                                if (Statistics.Enabled)
                                {
                                    Statistics.Lmped[depth]++;
                                }

                                continue;
                            }

                            /* futility pruning */
                            if (EngineConstants.EnableFutilityPruning && depth < FutilityMargin.Length)
                            {
                                if (!MoveUtil.IsPawnPush78(move))
                                {
                                    if (eval == Util.ShortMin)
                                    {
                                        eval = EvalUtil.GetScore(cb, threadData);
                                    }

                                    if (eval + FutilityMargin[depth] <= alpha)
                                    {
                                        if (Statistics.Enabled)
                                        {
                                            Statistics.Futile[depth]++;
                                        }

                                        continue;
                                    }
                                }
                            }
                        }
                        /* SEE Pruning */
                        else if (EngineConstants.EnableSeePruning && depth <= 6 && phase == PhaseAttacking
                                 && SeeUtil.GetSeeCaptureScore(cb, move) < -20 * depth * depth)
                        {
                            continue;
                        }
                    }

                    cb.DoMove(move);
                    movesPerformed++;

                    /* draw check */
                    if (cb.IsRepetition(move) || MaterialUtil.IsDrawByMaterial(cb))
                    {
                        score = EvalConstants.ScoreDraw;
                    }
                    else
                    {
                        score = alpha + 1; // initial is above alpha

                        var reduction = 1;
                        if (depth > 2 && movesPerformed > 1 && MoveUtil.IsQuiet(move) && !MoveUtil.IsPawnPush78(move))
                        {
                            reduction = LmrTable[Math.Min(depth, 63)][Math.Min(movesPerformed, 63)];
                            if (threadData.GetMoveScore() > 40)
                            {
                                reduction -= 1;
                            }

                            if (move == killer1Move || move == killer2Move || move == counterMove)
                            {
                                reduction -= 1;
                            }

                            if (!isPv)
                            {
                                reduction += 1;
                            }

                            reduction = Math.Min(depth - 1, Math.Max(reduction, 1));
                        }

                        /* LMR */
                        if (EngineConstants.EnableLmr && reduction != 1)
                        {
                            score = -CalculateBestMove(cb, threadData, ply + 1, depth - reduction, -alpha - 1, -alpha,
                                0);
                        }

                        /* PVS */
                        if (EngineConstants.EnablePvs && score > alpha && movesPerformed > 1)
                        {
                            score = -CalculateBestMove(cb, threadData, ply + 1, depth - 1, -alpha - 1, -alpha, 0);
                        }

                        /* normal bounds */
                        if (score > alpha)
                        {
                            score = -CalculateBestMove(cb, threadData, ply + 1, depth - 1, -beta, -alpha, 0);
                        }
                    }

                    cb.UndoMove(move);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = move;

                        if (ply == 0 && IsRunning)
                        {
                            threadData.SetBestMove(cb, bestMove, alphaOrig, beta, bestScore, depth);
                        }

                        alpha = Math.Max(alpha, score);
                        if (alpha >= beta)
                        {
                            if (Statistics.Enabled)
                            {
                                Statistics.FailHigh[Math.Min(movesPerformed - 1, Statistics.FailHigh.Length - 1)]++;
                            }

                            /* killer and history */
                            if (MoveUtil.IsQuiet(move) && cb.CheckingPieces == 0)
                            {
                                threadData.AddCounterMove(cb.ColorToMove, parentMove, move);
                                threadData.AddKillerMove(move, ply);
                                threadData.AddHhValue(cb.ColorToMove, move, depth);
                            }

                            phase += 10;
                            break;
                        }
                    }

                    if (MoveUtil.IsQuiet(move))
                    {
                        threadData.AddBfValue(cb.ColorToMove, move, depth);
                    }
                }

                phase++;
            }

            threadData.EndPly();

            /* checkmate or stalemate */
            if (movesPerformed == 0)
            {
                if (cb.CheckingPieces == 0)
                {
                    if (Statistics.Enabled)
                    {
                        Statistics.StaleMateCount++;
                    }

                    return EvalConstants.ScoreDraw;
                }
                else
                {
                    if (Statistics.Enabled)
                    {
                        Statistics.MateCount++;
                    }

                    return Util.ShortMin + ply;
                }
            }

            if (EngineConstants.Assert)
            {
                Assert.IsTrue(bestMove != 0);
            }

            // set tt-flag
            var flag = TtUtil.FlagExact;
            if (bestScore >= beta)
            {
                flag = TtUtil.FlagLower;
            }
            else if (bestScore <= alphaOrig)
            {
                flag = TtUtil.FlagUpper;
            }

            if (IsRunning)
            {
                TtUtil.AddValue(cb.ZobristKey, bestScore, ply, depth, flag, bestMove);
            }

            Statistics.SetBestMove(cb, bestMove, ttMove, ttEntry, flag, counterMove, killer1Move, killer2Move);

            if (EngineConstants.TestTtValues)
            {
                SearchTestUtil.TestTtValues(score, bestScore, depth, bestMove, flag, ttEntry, ply);
            }

            return bestScore;
        }

        private static int Extensions(ChessBoard cb)
        {
            /* check-extension */
            // TODO extend discovered checks?
            // TODO extend checks with SEE > 0?
            // TODO extend when mate-threat?
            if (!EngineConstants.EnableCheckExtension || cb.CheckingPieces == 0) return 0;
            if (Statistics.Enabled)
            {
                Statistics.CheckExtensions++;
            }

            return 1;
        }
    }
}