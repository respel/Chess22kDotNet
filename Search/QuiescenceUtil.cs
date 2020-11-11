using System;
using Chess22kDotNet.Engine;
using Chess22kDotNet.Eval;
using Chess22kDotNet.Move;

namespace Chess22kDotNet.Search
{
    public static class QuiescenceUtil
    {
        private const int FutilityMargin = 150;

        public static int CalculateBestMove(in ChessBoard cb, in ThreadData threadData, int alpha, in int beta)
        {
            if (Statistics.Enabled)
            {
                Statistics.QNodes++;
            }

            /* transposition-table */
            var ttValue = TtUtil.GetValue(cb.ZobristKey);
            var score = TtUtil.GetScore(ttValue, 64);
            if (ttValue != 0)
            {
                if (!EngineConstants.TestTtValues)
                {
                    switch (TtUtil.GetFlag(ttValue))
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

            if (cb.CheckingPieces != 0)
            {
                return alpha;
            }

            /* stand-pat check */
            var eval = EvalUtil.GetScore(cb, threadData);
            /* use tt value as eval */
            if (EngineConstants.UseTtScoreAsEval)
            {
                if (TtUtil.CanRefineEval(ttValue, eval, score))
                {
                    eval = score;
                }
            }

            if (eval >= beta)
            {
                return eval;
            }

            alpha = Math.Max(alpha, eval);

            threadData.StartPly();
            MoveGenerator.GenerateAttacks(threadData, cb);
            threadData.SetMvvlvaScores();
            threadData.Sort();

            while (threadData.HasNext())
            {
                var move = threadData.Next();

                // skip under promotions
                if (MoveUtil.IsPromotion(move))
                {
                    if (MoveUtil.GetMoveType(move) != MoveUtil.TypePromotionQ)
                    {
                        continue;
                    }
                }
                else if (EngineConstants.EnableQFutilityPruning
                         && eval + FutilityMargin + EvalConstants.Material[MoveUtil.GetAttackedPieceIndex(move)] <
                         alpha)
                {
                    // futility pruning
                    continue;
                }

                if (!cb.IsLegal(move))
                {
                    continue;
                }

                // skip bad-captures
                if (EngineConstants.EnableQPruneBadCaptures && !cb.IsDiscoveredMove(MoveUtil.GetFromIndex(move)) &&
                    SeeUtil.GetSeeCaptureScore(cb, move) <= 0)
                {
                    continue;
                }

                cb.DoMove(move);
                score = MaterialUtil.IsDrawByMaterial(cb)
                    ? EvalConstants.ScoreDraw
                    : -CalculateBestMove(cb, threadData, -beta, -alpha);
                cb.UndoMove(move);

                if (score >= beta)
                {
                    threadData.EndPly();
                    return score;
                }

                alpha = Math.Max(alpha, score);
            }

            threadData.EndPly();
            return alpha;
        }
    }
}