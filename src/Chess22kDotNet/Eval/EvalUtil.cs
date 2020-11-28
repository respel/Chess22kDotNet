using System;
using System.Numerics;
using Chess22kDotNet.Engine;
using Chess22kDotNet.Move;
using Chess22kDotNet.Search;
using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet.Eval
{
    public static class EvalUtil
    {
        public static readonly int PhaseTotal = 4 * EvalConstants.Phase[Knight] + 4 * EvalConstants.Phase[Bishop] +
                                                4 * EvalConstants.Phase[Rook]
                                                + 2 * EvalConstants.Phase[Queen];

        public static int GetScore(ChessBoard cb, ThreadData threadData)
        {
            if (Statistics.Enabled) Statistics.EvalNodes++;

            if (EngineConstants.EnableEvalCache && !EngineConstants.TestEvalCaches)
            {
                var score = EvalCacheUtil.GetScore(cb.ZobristKey, threadData.EvalCache);
                if (score != CacheMiss) return score;
            }

            return CalculateScore(cb, threadData);
        }

        public static int CalculateScore(ChessBoard cb, ThreadData threadData)
        {
            var score = MaterialUtil.ScoreUnknown;
            if (BitOperations.PopCount((ulong) cb.AllPieces) <= 5)
                score = MaterialUtil.IsDrawByMaterial(cb)
                    ? EvalConstants.ScoreDraw
                    : MaterialUtil.CalculateEndgameScore(cb);

            if (score == MaterialUtil.ScoreUnknown)
            {
                score = TaperedEval(cb, threadData);
                if (score > 25)
                    score = AdjustEndgame(cb, score, White, threadData.MaterialCache);
                else if (score < -25) score = AdjustEndgame(cb, score, Black, threadData.MaterialCache);
            }

            score *= ColorFactor[cb.ColorToMove];
            if (EngineConstants.TestEvalCaches)
            {
                var cachedScore = EvalCacheUtil.GetScore(cb.ZobristKey, threadData.EvalCache);
                if (cachedScore != CacheMiss)
                    if (cachedScore != score)
                        throw new ArgumentException($"Cached eval score != score: {cachedScore}, {score}");
            }

            EvalCacheUtil.AddValue(cb.ZobristKey, score, threadData.EvalCache);

            if (EngineConstants.TestEvalValues) ChessBoardTestUtil.CompareScores(cb);

            return score;
        }

        private static int AdjustEndgame(ChessBoard cb, int score, int color, int[] materialCache)
        {
            if (BitOperations.PopCount((ulong) cb.Pieces[color][All]) > 3) return score;

            if (MaterialUtil.HasPawnsOrQueens(cb.MaterialKey, color)) return score;

            switch (BitOperations.PopCount((ulong) cb.Pieces[color][All]))
            {
                case 1:
                    return EvalConstants.ScoreDraw;
                case 2:
                    if (cb.Pieces[color][Rook] == 0) return EvalConstants.ScoreDraw;

                    goto case 3;
                // fall-through
                case 3:
                    if (MaterialUtil.hasOnlyNights(cb.MaterialKey, color)) return EvalConstants.ScoreDraw;

                    if (GetImbalances(cb, materialCache) * ColorFactor[color] <
                        EvalConstants.OtherScores[EvalConstants.IxDrawish])
                        return score / 8;

                    break;
            }

            return score;
        }

        private static int TaperedEval(ChessBoard cb, ThreadData threadData)
        {
            var pawnScore = GetPawnScores(cb, threadData.PawnCache);
            var mgEgScore = CalculateMobilityScoresAndSetAttacks(cb) + CalculateThreats(cb) +
                            CalculatePawnShieldBonus(cb);
            var phaseIndependentScore = CalculateOthers(cb) + GetImbalances(cb, threadData.MaterialCache);

            var scoreMg = cb.Phase == PhaseTotal
                ? 0
                : GetMgScore(mgEgScore + cb.PsqtScore) + pawnScore + KingSafetyEval.CalculateScores(cb) +
                  CalculateSpace(cb) + phaseIndependentScore;
            var scoreEg = GetEgScore(mgEgScore + cb.PsqtScore) + pawnScore + PassedPawnEval.CalculateScores(cb) +
                          phaseIndependentScore;

            return (scoreMg * (PhaseTotal - cb.Phase) + scoreEg * cb.Phase) / PhaseTotal / CalculateScaleFactor(cb);
        }

        public static int Score(int mgScore, int egScore)
        {
            return (mgScore << 16) + egScore;
        }

        public static int GetMgScore(int score)
        {
            return (score + 0x8000) >> 16;
        }

        public static int GetEgScore(int score)
        {
            return (short) (score & 0xffff);
        }

        private static int CalculateScaleFactor(ChessBoard cb)
        {
            // opposite bishops endgame?
            if (!MaterialUtil.OppositeBishops(cb.MaterialKey)) return 1;
            return (cb.Pieces[White][Bishop] & Bitboard.BlackSquares) == 0 ==
                   ((cb.Pieces[Black][Bishop] & Bitboard.WhiteSquares) == 0)
                ? 2
                : 1;

            // TODO rook and pawns without passed pawns
        }

        public static int CalculateSpace(ChessBoard cb)
        {
            if (!MaterialUtil.HasPawns(cb.MaterialKey)) return 0;

            var score = 0;

            score += EvalConstants.OtherScores[EvalConstants.IxSpace]
                     * BitOperations.PopCount((ulong) (Util.RightTripleShift(cb.Pieces[White][Pawn], 8) &
                                                       (cb.Pieces[White][Knight] | cb.Pieces[White][Bishop]) &
                                                       Bitboard.Rank234));
            score -= EvalConstants.OtherScores[EvalConstants.IxSpace]
                     * BitOperations.PopCount((ulong) ((cb.Pieces[Black][Pawn] << 8) &
                                                       (cb.Pieces[Black][Knight] | cb.Pieces[Black][Bishop]) &
                                                       Bitboard.Rank567));

            // idea taken from Laser
            var space = Util.RightTripleShift(cb.Pieces[White][Pawn], 8);
            space |= Util.RightTripleShift(space, 8) | Util.RightTripleShift(space, 16);
            score += EvalConstants.Space[BitOperations.PopCount((ulong) cb.Pieces[White][All])]
                     * BitOperations.PopCount((ulong) (space & ~cb.Pieces[White][Pawn] & ~cb.Attacks[Black][Pawn] &
                                                       Bitboard.FileCdef));
            space = cb.Pieces[Black][Pawn] << 8;
            space |= (space << 8) | (space << 16);
            score -= EvalConstants.Space[BitOperations.PopCount((ulong) cb.Pieces[Black][All])]
                     * BitOperations.PopCount((ulong) (space & ~cb.Pieces[Black][Pawn] & ~cb.Attacks[White][Pawn] &
                                                       Bitboard.FileCdef));

            return score;
        }

        public static int GetPawnScores(ChessBoard cb, long[] pawnCache)
        {
            if (!EngineConstants.TestEvalCaches)
            {
                var cachedScore = PawnCacheUtil.UpdateBoardAndGetScore(cb, pawnCache);
                if (cachedScore != CacheMiss) return cachedScore;
            }

            var score = CalculatePawnScores(cb);
            PawnCacheUtil.AddValue(cb.PawnZobristKey, score, cb.PassedPawnsAndOutposts, pawnCache);
            return score;
        }

        private static int CalculatePawnScores(ChessBoard cb)
        {
            var score = 0;

            // penalty for doubled pawns
            for (var i = 0; i < 8; i++)
            {
                if (BitOperations.PopCount((ulong) (cb.Pieces[White][Pawn] & Bitboard.Files[i])) > 1)
                    score -= EvalConstants.PawnScores[EvalConstants.IxPawnDouble];

                if (BitOperations.PopCount((ulong) (cb.Pieces[Black][Pawn] & Bitboard.Files[i])) > 1)
                    score += EvalConstants.PawnScores[EvalConstants.IxPawnDouble];
            }

            // bonus for connected pawns
            var pawns = Bitboard.GetWhitePawnAttacks(cb.Pieces[White][Pawn]) & cb.Pieces[White][Pawn];
            while (pawns != 0)
            {
                score += EvalConstants.PawnConnected[BitOperations.TrailingZeroCount(pawns) / 8];
                pawns &= pawns - 1;
            }

            pawns = Bitboard.GetBlackPawnAttacks(cb.Pieces[Black][Pawn]) & cb.Pieces[Black][Pawn];
            while (pawns != 0)
            {
                score -= EvalConstants.PawnConnected[7 - BitOperations.TrailingZeroCount(pawns) / 8];
                pawns &= pawns - 1;
            }

            // bonus for neighbour pawns
            pawns = Bitboard.GetPawnNeighbours(cb.Pieces[White][Pawn]) & cb.Pieces[White][Pawn];
            while (pawns != 0)
            {
                score += EvalConstants.PawnNeighbour[BitOperations.TrailingZeroCount(pawns) / 8];
                pawns &= pawns - 1;
            }

            pawns = Bitboard.GetPawnNeighbours(cb.Pieces[Black][Pawn]) & cb.Pieces[Black][Pawn];
            while (pawns != 0)
            {
                score -= EvalConstants.PawnNeighbour[7 - BitOperations.TrailingZeroCount(pawns) / 8];
                pawns &= pawns - 1;
            }

            // set outposts
            cb.PassedPawnsAndOutposts = 0;
            pawns = Bitboard.GetWhitePawnAttacks(cb.Pieces[White][Pawn]) & ~cb.Pieces[White][Pawn] &
                    ~cb.Pieces[Black][Pawn];
            while (pawns != 0)
            {
                if ((Bitboard.GetWhiteAdjacentMask(BitOperations.TrailingZeroCount(pawns)) & cb.Pieces[Black][Pawn]) ==
                    0)
                    cb.PassedPawnsAndOutposts |= pawns & -pawns;

                pawns &= pawns - 1;
            }

            pawns = Bitboard.GetBlackPawnAttacks(cb.Pieces[Black][Pawn]) & ~cb.Pieces[White][Pawn] &
                    ~cb.Pieces[Black][Pawn];
            while (pawns != 0)
            {
                if ((Bitboard.GetBlackAdjacentMask(BitOperations.TrailingZeroCount(pawns)) & cb.Pieces[White][Pawn]) ==
                    0)
                    cb.PassedPawnsAndOutposts |= pawns & -pawns;

                pawns &= pawns - 1;
            }

            int index;

            // white
            pawns = cb.Pieces[White][Pawn];
            while (pawns != 0)
            {
                index = BitOperations.TrailingZeroCount(pawns);

                // isolated pawns
                if ((Bitboard.FilesAdjacent[index & 7] & cb.Pieces[White][Pawn]) == 0)
                    score -= EvalConstants.PawnScores[EvalConstants.IxPawnIsolated];

                // backward pawns
                else if ((Bitboard.GetBlackAdjacentMask(index + 8) & cb.Pieces[White][Pawn]) == 0)
                    if ((StaticMoves.PawnAttacks[White][index + 8] & cb.Pieces[Black][Pawn]) != 0)
                        if ((Bitboard.Files[index & 7] & cb.Pieces[Black][Pawn]) == 0)
                            score -= EvalConstants.PawnScores[EvalConstants.IxPawnBackward];

                // pawn defending 2 pawns
                if (BitOperations.PopCount((ulong) (StaticMoves.PawnAttacks[White][index] & cb.Pieces[White][Pawn])) ==
                    2)
                    score -= EvalConstants.PawnScores[EvalConstants.IxPawnInverse];

                // set passed pawns
                if ((Bitboard.GetWhitePassedPawnMask(index) & cb.Pieces[Black][Pawn]) == 0)
                    cb.PassedPawnsAndOutposts |= pawns & -pawns;

                // candidate passed pawns (no pawns in front, more friendly pawns behind and adjacent than enemy pawns)
                else if (63 - BitOperations.LeadingZeroCount(
                    (ulong) ((cb.Pieces[White][Pawn] | cb.Pieces[Black][Pawn]) &
                             Bitboard.Files[index & 7])) == index)
                    if (BitOperations.PopCount((ulong) (cb.Pieces[White][Pawn] &
                                                        Bitboard.GetBlackPassedPawnMask(index + 8))) >=
                        BitOperations.PopCount(
                            (ulong) (cb.Pieces[Black][Pawn] & Bitboard.GetWhitePassedPawnMask(index))))
                        score += EvalConstants.PassedCandidate[index / 8];

                pawns &= pawns - 1;
            }

            // black
            pawns = cb.Pieces[Black][Pawn];
            while (pawns != 0)
            {
                index = BitOperations.TrailingZeroCount(pawns);

                // isolated pawns
                if ((Bitboard.FilesAdjacent[index & 7] & cb.Pieces[Black][Pawn]) == 0)
                    score += EvalConstants.PawnScores[EvalConstants.IxPawnIsolated];

                // backward pawns
                else if ((Bitboard.GetWhiteAdjacentMask(index - 8) & cb.Pieces[Black][Pawn]) == 0)
                    if ((StaticMoves.PawnAttacks[Black][index - 8] & cb.Pieces[White][Pawn]) != 0)
                        if ((Bitboard.Files[index & 7] & cb.Pieces[White][Pawn]) == 0)
                            score += EvalConstants.PawnScores[EvalConstants.IxPawnBackward];

                // pawn defending 2 pawns
                if (BitOperations.PopCount((ulong) (StaticMoves.PawnAttacks[Black][index] & cb.Pieces[Black][Pawn])) ==
                    2)
                    score += EvalConstants.PawnScores[EvalConstants.IxPawnInverse];

                // set passed pawns
                if ((Bitboard.GetBlackPassedPawnMask(index) & cb.Pieces[White][Pawn]) == 0)
                    cb.PassedPawnsAndOutposts |= pawns & -pawns;

                // candidate passers
                else if (BitOperations.TrailingZeroCount((cb.Pieces[White][Pawn] | cb.Pieces[Black][Pawn]) &
                                                         Bitboard.Files[index & 7]) == index)
                    if (BitOperations.PopCount((ulong) (cb.Pieces[Black][Pawn] &
                                                        Bitboard.GetWhitePassedPawnMask(index - 8))) >=
                        BitOperations.PopCount(
                            (ulong) (cb.Pieces[White][Pawn] & Bitboard.GetBlackPassedPawnMask(index))))
                        score -= EvalConstants.PassedCandidate[7 - index / 8];

                pawns &= pawns - 1;
            }

            return score;
        }

        public static int GetImbalances(ChessBoard cb, int[] materialCache)
        {
            if (!EngineConstants.TestEvalCaches)
            {
                var cachedScore = MaterialCacheUtil.GetScore(cb.MaterialKey, materialCache);
                if (cachedScore != CacheMiss) return cachedScore;
            }

            var score = CalculateImbalances(cb);
            MaterialCacheUtil.AddValue(cb.MaterialKey, score, materialCache);
            return score;
        }

        private static int CalculateImbalances(ChessBoard cb)
        {
            var score = 0;

            // material
            score += CalculateMaterialScore(cb);

            // knights and pawns
            score += BitOperations.PopCount((ulong) cb.Pieces[White][Knight]) *
                     EvalConstants.KnightPawn[BitOperations.PopCount((ulong) cb.Pieces[White][Pawn])];
            score -= BitOperations.PopCount((ulong) cb.Pieces[Black][Knight]) *
                     EvalConstants.KnightPawn[BitOperations.PopCount((ulong) cb.Pieces[Black][Pawn])];

            // rooks and pawns
            score += BitOperations.PopCount((ulong) cb.Pieces[White][Rook]) *
                     EvalConstants.RookPawn[BitOperations.PopCount((ulong) cb.Pieces[White][Pawn])];
            score -= BitOperations.PopCount((ulong) cb.Pieces[Black][Rook]) *
                     EvalConstants.RookPawn[BitOperations.PopCount((ulong) cb.Pieces[Black][Pawn])];

            // double bishop
            if (BitOperations.PopCount((ulong) cb.Pieces[White][Bishop]) == 2)
                score += EvalConstants.ImbalanceScores[EvalConstants.IxBishopDouble];

            if (BitOperations.PopCount((ulong) cb.Pieces[Black][Bishop]) == 2)
                score -= EvalConstants.ImbalanceScores[EvalConstants.IxBishopDouble];

            // queen and nights
            if (cb.Pieces[White][Queen] != 0)
                score += BitOperations.PopCount((ulong) cb.Pieces[White][Knight]) *
                         EvalConstants.ImbalanceScores[EvalConstants.IxQueenNight];

            if (cb.Pieces[Black][Queen] != 0)
                score -= BitOperations.PopCount((ulong) cb.Pieces[Black][Knight]) *
                         EvalConstants.ImbalanceScores[EvalConstants.IxQueenNight];

            // rook pair
            if (BitOperations.PopCount((ulong) cb.Pieces[White][Rook]) > 1)
                score += EvalConstants.ImbalanceScores[EvalConstants.IxRookPair];

            if (BitOperations.PopCount((ulong) cb.Pieces[Black][Rook]) > 1)
                score -= EvalConstants.ImbalanceScores[EvalConstants.IxRookPair];

            return score;
        }

        public static int CalculateThreats(ChessBoard cb)
        {
            var score = 0;
            var whites = cb.Pieces[White][All];
            var whitePawns = cb.Pieces[White][Pawn];
            var blacks = cb.Pieces[Black][All];
            var blackPawns = cb.Pieces[Black][Pawn];
            var whiteAttacks = cb.Attacks[White][All];
            var whitePawnAttacks = cb.Attacks[White][Pawn];
            var whiteMinorAttacks = cb.Attacks[White][Knight] | cb.Attacks[White][Bishop];
            var blackAttacks = cb.Attacks[Black][All];
            var blackPawnAttacks = cb.Attacks[Black][Pawn];
            var blackMinorAttacks = cb.Attacks[Black][Knight] | cb.Attacks[Black][Bishop];

            // double attacked pieces
            var piece = cb.DoubleAttacks[White] & blacks;
            while (piece != 0)
            {
                score += EvalConstants.DoubleAttacked[cb.PieceIndexes[BitOperations.TrailingZeroCount(piece)]];
                piece &= piece - 1;
            }

            piece = cb.DoubleAttacks[Black] & whites;
            while (piece != 0)
            {
                score -= EvalConstants.DoubleAttacked[cb.PieceIndexes[BitOperations.TrailingZeroCount(piece)]];
                piece &= piece - 1;
            }

            if (MaterialUtil.HasPawns(cb.MaterialKey))
            {
                // unused outposts
                score += BitOperations.PopCount((ulong) (cb.PassedPawnsAndOutposts & cb.EmptySpaces &
                                                         whiteMinorAttacks & whitePawnAttacks))
                         * EvalConstants.Threats[EvalConstants.IxUnusedOutpost];
                score -= BitOperations.PopCount((ulong) (cb.PassedPawnsAndOutposts & cb.EmptySpaces &
                                                         blackMinorAttacks & blackPawnAttacks))
                         * EvalConstants.Threats[EvalConstants.IxUnusedOutpost];

                // pawn push threat
                piece = (whitePawns << 8) & cb.EmptySpaces & ~blackAttacks;
                score += BitOperations.PopCount((ulong) (Bitboard.GetWhitePawnAttacks(piece) & blacks)) *
                         EvalConstants.Threats[EvalConstants.IxPawnPushThreat];
                piece = Util.RightTripleShift(blackPawns, 8) & cb.EmptySpaces & ~whiteAttacks;
                score -= BitOperations.PopCount((ulong) (Bitboard.GetBlackPawnAttacks(piece) & whites)) *
                         EvalConstants.Threats[EvalConstants.IxPawnPushThreat];

                // piece attacked by pawn
                score += BitOperations.PopCount((ulong) (whitePawnAttacks & blacks & ~blackPawns)) *
                         EvalConstants.Threats[EvalConstants.IxPawnAttacks];
                score -= BitOperations.PopCount((ulong) (blackPawnAttacks & whites & ~whitePawns)) *
                         EvalConstants.Threats[EvalConstants.IxPawnAttacks];

                // multiple pawn attacks possible
                if (BitOperations.PopCount((ulong) (whitePawnAttacks & blacks)) > 1)
                    score += EvalConstants.Threats[EvalConstants.IxMultiplePawnAttacks];

                if (BitOperations.PopCount((ulong) (blackPawnAttacks & whites)) > 1)
                    score -= EvalConstants.Threats[EvalConstants.IxMultiplePawnAttacks];

                // pawn attacked
                score += BitOperations.PopCount((ulong) (whiteAttacks & blackPawns)) *
                         EvalConstants.Threats[EvalConstants.IxPawnAttacked];
                score -= BitOperations.PopCount((ulong) (blackAttacks & whitePawns)) *
                         EvalConstants.Threats[EvalConstants.IxPawnAttacked];
            }

            // minors attacked and not defended by a pawn
            score += BitOperations.PopCount((ulong) (whiteAttacks &
                                                     (cb.Pieces[Black][Knight] |
                                                      (cb.Pieces[Black][Bishop] & ~blackAttacks))))
                     * EvalConstants.Threats[EvalConstants.IxMajorAttacked];
            score -= BitOperations.PopCount((ulong) (blackAttacks &
                                                     (cb.Pieces[White][Knight] |
                                                      (cb.Pieces[White][Bishop] & ~whiteAttacks))))
                     * EvalConstants.Threats[EvalConstants.IxMajorAttacked];

            if (cb.Pieces[Black][Queen] != 0)
            {
                // queen attacked by rook
                score += BitOperations.PopCount((ulong) (cb.Attacks[White][Rook] & cb.Pieces[Black][Queen])) *
                         EvalConstants.Threats[EvalConstants.IxQueenAttacked];
                // queen attacked by minors
                score += BitOperations.PopCount((ulong) (whiteMinorAttacks & cb.Pieces[Black][Queen])) *
                         EvalConstants.Threats[EvalConstants.IxQueenAttackedMinor];
            }

            if (cb.Pieces[White][Queen] != 0)
            {
                // queen attacked by rook
                score -= BitOperations.PopCount((ulong) (cb.Attacks[Black][Rook] & cb.Pieces[White][Queen])) *
                         EvalConstants.Threats[EvalConstants.IxQueenAttacked];
                // queen attacked by minors
                score -= BitOperations.PopCount((ulong) (blackMinorAttacks & cb.Pieces[White][Queen])) *
                         EvalConstants.Threats[EvalConstants.IxQueenAttackedMinor];
            }

            // rook attacked by minors
            score += BitOperations.PopCount((ulong) (whiteMinorAttacks & cb.Pieces[Black][Rook])) *
                     EvalConstants.Threats[EvalConstants.IxRookAttacked];
            score -= BitOperations.PopCount((ulong) (blackMinorAttacks & cb.Pieces[White][Rook])) *
                     EvalConstants.Threats[EvalConstants.IxRookAttacked];

            return score;
        }

        public static int CalculateOthers(ChessBoard cb)
        {
            var score = 0;
            long piece;

            var whites = cb.Pieces[White][All];
            var whitePawns = cb.Pieces[White][Pawn];
            var blacks = cb.Pieces[Black][All];
            var blackPawns = cb.Pieces[Black][Pawn];
            var whitePawnAttacks = cb.Attacks[White][Pawn];
            var blackPawnAttacks = cb.Attacks[Black][Pawn];

            // side to move
            score += ColorFactor[cb.ColorToMove] * EvalConstants.SideToMoveBonus;

            // WHITE ROOK
            if (cb.Pieces[White][Rook] != 0)
            {
                piece = cb.Pieces[White][Rook];

                // rook battery (same file)
                if (BitOperations.PopCount((ulong) piece) == 2)
                    if ((BitOperations.TrailingZeroCount(piece) & 7) ==
                        ((63 - BitOperations.LeadingZeroCount((ulong) piece)) & 7))
                        score += EvalConstants.OtherScores[EvalConstants.IxRookBattery];

                // rook on 7th, king on 8th
                if (cb.KingIndex[Black] >= 56 && (piece & Bitboard.Rank7) != 0)
                    score += BitOperations.PopCount((ulong) (piece & Bitboard.Rank7)) *
                             EvalConstants.OtherScores[EvalConstants.IxRook7ThRank];

                // prison
                if ((piece & Bitboard.Rank1) != 0)
                {
                    var trapped = piece & EvalConstants.RookPrison[cb.KingIndex[White]];
                    if (trapped != 0)
                        if ((((trapped << 8) | (trapped << 16)) & whitePawns) != 0)
                            score += EvalConstants.OtherScores[EvalConstants.IxRookTrapped];
                }

                // rook on open-file (no pawns) and semi-open-file (no friendly pawns)
                while (piece != 0)
                {
                    if ((whitePawns & Bitboard.GetFile(piece)) == 0)
                    {
                        if ((blackPawns & Bitboard.GetFile(piece)) == 0)
                            score += EvalConstants.OtherScores[EvalConstants.IxRookFileOpen];
                        else if ((blackPawns & blackPawnAttacks & Bitboard.GetFile(piece)) == 0)
                            score += EvalConstants.OtherScores[EvalConstants.IxRookFileSemiOpenIsolated];
                        else
                            score += EvalConstants.OtherScores[EvalConstants.IxRookFileSemiOpen];
                    }

                    piece &= piece - 1;
                }
            }

            // BLACK ROOK
            if (cb.Pieces[Black][Rook] != 0)
            {
                piece = cb.Pieces[Black][Rook];

                // rook battery (same file)
                if (BitOperations.PopCount((ulong) piece) == 2)
                    if ((BitOperations.TrailingZeroCount(piece) & 7) ==
                        ((63 - BitOperations.LeadingZeroCount((ulong) piece)) & 7))
                        score -= EvalConstants.OtherScores[EvalConstants.IxRookBattery];

                // rook on 2nd, king on 1st
                if (cb.KingIndex[White] <= 7 && (piece & Bitboard.Rank2) != 0)
                    score -= BitOperations.PopCount((ulong) (piece & Bitboard.Rank2)) *
                             EvalConstants.OtherScores[EvalConstants.IxRook7ThRank];

                // prison
                if ((piece & Bitboard.Rank8) != 0)
                {
                    var trapped = piece & EvalConstants.RookPrison[cb.KingIndex[Black]];
                    if (trapped != 0)
                        if (((Util.RightTripleShift(trapped, 8) | Util.RightTripleShift(trapped, 16)) & blackPawns) !=
                            0)
                            score -= EvalConstants.OtherScores[EvalConstants.IxRookTrapped];
                }

                // rook on open-file (no pawns) and semi-open-file (no friendly pawns)
                while (piece != 0)
                {
                    // TODO JITWatch unpredictable branch
                    if ((blackPawns & Bitboard.GetFile(piece)) == 0)
                    {
                        if ((whitePawns & Bitboard.GetFile(piece)) == 0)
                            score -= EvalConstants.OtherScores[EvalConstants.IxRookFileOpen];
                        else if ((whitePawns & whitePawnAttacks & Bitboard.GetFile(piece)) == 0)
                            score -= EvalConstants.OtherScores[EvalConstants.IxRookFileSemiOpenIsolated];
                        else
                            score -= EvalConstants.OtherScores[EvalConstants.IxRookFileSemiOpen];
                    }

                    piece &= piece - 1;
                }
            }

            // WHITE BISHOP
            if (cb.Pieces[White][Bishop] != 0)
            {
                // bishop outpost: protected by a pawn, cannot be attacked by enemy pawns
                piece = cb.Pieces[White][Bishop] & cb.PassedPawnsAndOutposts & whitePawnAttacks;
                if (piece != 0)
                    score += BitOperations.PopCount((ulong) piece) * EvalConstants.OtherScores[EvalConstants.IxOutpost];

                piece = cb.Pieces[White][Bishop];
                if ((piece & Bitboard.WhiteSquares) != 0)
                {
                    // pawns on same color as bishop
                    score += EvalConstants.BishopPawn[
                        BitOperations.PopCount((ulong) (whitePawns & Bitboard.WhiteSquares))];

                    // attacking center squares
                    if (BitOperations.PopCount((ulong) (cb.Attacks[White][Bishop] & Bitboard.E4D5)) == 2)
                        score += EvalConstants.OtherScores[EvalConstants.IxBishopLong];
                }

                if ((piece & Bitboard.BlackSquares) != 0)
                {
                    // pawns on same color as bishop
                    score += EvalConstants.BishopPawn[
                        BitOperations.PopCount((ulong) (whitePawns & Bitboard.BlackSquares))];

                    // attacking center squares
                    if (BitOperations.PopCount((ulong) (cb.Attacks[White][Bishop] & Bitboard.D4E5)) == 2)
                        score += EvalConstants.OtherScores[EvalConstants.IxBishopLong];
                }

                // prison
                piece &= Bitboard.Rank2;
                while (piece != 0)
                {
                    if (BitOperations.PopCount(
                        (ulong) (EvalConstants.BishopPrison[BitOperations.TrailingZeroCount(piece)] & blackPawns)) == 2)
                        score += EvalConstants.OtherScores[EvalConstants.IxBishopPrison];

                    piece &= piece - 1;
                }
            }

            // BLACK BISHOP
            if (cb.Pieces[Black][Bishop] != 0)
            {
                // bishop outpost: protected by a pawn, cannot be attacked by enemy pawns
                piece = cb.Pieces[Black][Bishop] & cb.PassedPawnsAndOutposts & blackPawnAttacks;
                if (piece != 0)
                    score -= BitOperations.PopCount((ulong) piece) * EvalConstants.OtherScores[EvalConstants.IxOutpost];

                piece = cb.Pieces[Black][Bishop];
                if ((piece & Bitboard.WhiteSquares) != 0)
                {
                    // penalty for many pawns on same color as bishop
                    score -= EvalConstants.BishopPawn[
                        BitOperations.PopCount((ulong) (blackPawns & Bitboard.WhiteSquares))];

                    // bonus for attacking center squares
                    if (BitOperations.PopCount((ulong) (cb.Attacks[Black][Bishop] & Bitboard.E4D5)) == 2)
                        score -= EvalConstants.OtherScores[EvalConstants.IxBishopLong];
                }

                if ((piece & Bitboard.BlackSquares) != 0)
                {
                    // penalty for many pawns on same color as bishop
                    score -= EvalConstants.BishopPawn[
                        BitOperations.PopCount((ulong) (blackPawns & Bitboard.BlackSquares))];

                    // bonus for attacking center squares
                    if (BitOperations.PopCount((ulong) (cb.Attacks[Black][Bishop] & Bitboard.D4E5)) == 2)
                        score -= EvalConstants.OtherScores[EvalConstants.IxBishopLong];
                }

                // prison
                piece &= Bitboard.Rank7;
                while (piece != 0)
                {
                    if (BitOperations.PopCount(
                        (ulong) (EvalConstants.BishopPrison[BitOperations.TrailingZeroCount(piece)] & whitePawns)) == 2)
                        score -= EvalConstants.OtherScores[EvalConstants.IxBishopPrison];

                    piece &= piece - 1;
                }
            }

            // pieces supporting our pawns
            piece = (whitePawns << 8) & whites;
            while (piece != 0)
            {
                score += EvalConstants.PawnBlockage[Util.RightTripleShift(BitOperations.TrailingZeroCount(piece), 3)];
                piece &= piece - 1;
            }

            piece = Util.RightTripleShift(blackPawns, 8) & blacks;
            while (piece != 0)
            {
                score -= EvalConstants.PawnBlockage[7 - BitOperations.TrailingZeroCount(piece) / 8];
                piece &= piece - 1;
            }

            // knight outpost: protected by a pawn, cannot be attacked by enemy pawns
            piece = cb.Pieces[White][Knight] & cb.PassedPawnsAndOutposts & whitePawnAttacks;
            if (piece != 0)
                score += BitOperations.PopCount((ulong) piece) * EvalConstants.OtherScores[EvalConstants.IxOutpost];

            piece = cb.Pieces[Black][Knight] & cb.PassedPawnsAndOutposts & blackPawnAttacks;
            if (piece != 0)
                score -= BitOperations.PopCount((ulong) piece) * EvalConstants.OtherScores[EvalConstants.IxOutpost];

            // pinned-pieces
            if (cb.PinnedPieces != 0)
            {
                piece = cb.PinnedPieces & whites;
                while (piece != 0)
                {
                    score += EvalConstants.Pinned[cb.PieceIndexes[BitOperations.TrailingZeroCount(piece)]];
                    piece &= piece - 1;
                }

                piece = cb.PinnedPieces & blacks;
                while (piece != 0)
                {
                    score -= EvalConstants.Pinned[cb.PieceIndexes[BitOperations.TrailingZeroCount(piece)]];
                    piece &= piece - 1;
                }
            }

            // discovered-pieces
            if (cb.DiscoveredPieces != 0)
            {
                piece = cb.DiscoveredPieces & whites;
                while (piece != 0)
                {
                    score += EvalConstants.Discovered[cb.PieceIndexes[BitOperations.TrailingZeroCount(piece)]];
                    piece &= piece - 1;
                }

                piece = cb.DiscoveredPieces & blacks;
                while (piece != 0)
                {
                    score -= EvalConstants.Discovered[cb.PieceIndexes[BitOperations.TrailingZeroCount(piece)]];
                    piece &= piece - 1;
                }
            }

            if (cb.CastlingRights == 0) return score;
            score += BitOperations.PopCount((ulong) (cb.CastlingRights & 12)) *
                     EvalConstants.OtherScores[EvalConstants.IxCastling];
            score -= BitOperations.PopCount((ulong) (cb.CastlingRights & 3)) *
                     EvalConstants.OtherScores[EvalConstants.IxCastling];

            return score;
        }

        public static int CalculatePawnShieldBonus(ChessBoard cb)
        {
            if (!MaterialUtil.HasPawns(cb.MaterialKey)) return 0;

            int file;

            var whiteScore = 0;
            var piece = cb.Pieces[White][Pawn] & KingArea[cb.KingIndex[White]] & ~cb.Attacks[Black][Pawn];
            while (piece != 0)
            {
                file = BitOperations.TrailingZeroCount(piece) & 7;
                whiteScore +=
                    EvalConstants.ShieldBonus[Math.Min(7 - file, file)][
                        Util.RightTripleShift(BitOperations.TrailingZeroCount(piece), 3)];
                piece &= ~Bitboard.Files[file];
            }

            if (cb.Pieces[Black][Queen] == 0) whiteScore /= 2;

            var blackScore = 0;
            piece = cb.Pieces[Black][Pawn] & KingArea[cb.KingIndex[Black]] & ~cb.Attacks[White][Pawn];
            while (piece != 0)
            {
                file = (63 - BitOperations.LeadingZeroCount((ulong) piece)) & 7;
                blackScore +=
                    EvalConstants.ShieldBonus[Math.Min(7 - file, file)]
                        [7 - (63 - BitOperations.LeadingZeroCount((ulong) piece)) / 8];
                piece &= ~Bitboard.Files[file];
            }

            if (cb.Pieces[White][Queen] == 0) blackScore /= 2;

            return whiteScore - blackScore;
        }

        public static int CalculateMobilityScoresAndSetAttacks(ChessBoard cb)
        {
            cb.ClearEvalAttacks();

            for (var color = White; color <= Black; color++)
            {
                var kingArea = KingArea[cb.KingIndex[1 - color]];
                var piece = cb.Pieces[color][Pawn] & ~cb.PinnedPieces;
                while (piece != 0)
                {
                    cb.UpdatePawnAttacks(StaticMoves.PawnAttacks[color][BitOperations.TrailingZeroCount(piece)], color);
                    piece &= piece - 1;
                }

                cb.UpdatePawnAttacks(color, kingArea);

                piece = cb.Pieces[color][Pawn] & cb.PinnedPieces;
                while (piece != 0)
                {
                    cb.UpdateAttacks(StaticMoves.PawnAttacks[color][BitOperations.TrailingZeroCount(piece)]
                                     & PinnedMovement[BitOperations.TrailingZeroCount(piece)][cb.KingIndex[color]],
                        Pawn,
                        color, kingArea);
                    piece &= piece - 1;
                }
            }

            var score = 0;
            long moves;
            for (var color = White; color <= Black; color++)
            {
                var tempScore = 0;

                var kingArea = KingArea[cb.KingIndex[1 - color]];
                var safeMoves = ~cb.Pieces[color][All] & ~cb.Attacks[1 - color][Pawn];

                // knights
                var piece = cb.Pieces[color][Knight] & ~cb.PinnedPieces;
                while (piece != 0)
                {
                    moves = StaticMoves.KnightMoves[BitOperations.TrailingZeroCount(piece)];
                    cb.UpdateAttacks(moves, Knight, color, kingArea);
                    tempScore += EvalConstants.MobilityKnight[BitOperations.PopCount((ulong) (moves & safeMoves))];
                    piece &= piece - 1;
                }

                // bishops
                piece = cb.Pieces[color][Bishop];
                while (piece != 0)
                {
                    moves = MagicUtil.GetBishopMoves(BitOperations.TrailingZeroCount(piece),
                        cb.AllPieces ^ cb.Pieces[color][Queen]);
                    cb.UpdateAttacks(moves, Bishop, color, kingArea);
                    tempScore += EvalConstants.MobilityBishop[BitOperations.PopCount((ulong) (moves & safeMoves))];
                    piece &= piece - 1;
                }

                // rooks
                piece = cb.Pieces[color][Rook];
                while (piece != 0)
                {
                    moves = MagicUtil.GetRookMoves(BitOperations.TrailingZeroCount(piece),
                        cb.AllPieces ^ cb.Pieces[color][Rook] ^ cb.Pieces[color][Queen]);
                    cb.UpdateAttacks(moves, Rook, color, kingArea);
                    tempScore += EvalConstants.MobilityRook[BitOperations.PopCount((ulong) (moves & safeMoves))];
                    piece &= piece - 1;
                }

                // queens
                piece = cb.Pieces[color][Queen];
                while (piece != 0)
                {
                    moves = MagicUtil.GetQueenMoves(BitOperations.TrailingZeroCount(piece), cb.AllPieces);
                    cb.UpdateAttacks(moves, Queen, color, kingArea);
                    tempScore += EvalConstants.MobilityQueen[BitOperations.PopCount((ulong) (moves & safeMoves))];
                    piece &= piece - 1;
                }

                score += tempScore * ColorFactor[color];
            }

            // TODO king-attacks with or without enemy attacks?
            // WHITE king
            moves = StaticMoves.KingMoves[cb.KingIndex[White]] & ~StaticMoves.KingMoves[cb.KingIndex[Black]];
            cb.Attacks[White][King] = moves;
            cb.DoubleAttacks[White] |= cb.Attacks[White][All] & moves;
            cb.Attacks[White][All] |= moves;
            score += EvalConstants.MobilityKing[
                BitOperations.PopCount((ulong) (moves & ~cb.Pieces[White][All] & ~cb.Attacks[Black][All]))];

            // BLACK king
            moves = StaticMoves.KingMoves[cb.KingIndex[Black]] & ~StaticMoves.KingMoves[cb.KingIndex[White]];
            cb.Attacks[Black][King] = moves;
            cb.DoubleAttacks[Black] |= cb.Attacks[Black][All] & moves;
            cb.Attacks[Black][All] |= moves;
            score -= EvalConstants.MobilityKing[
                BitOperations.PopCount((ulong) (moves & ~cb.Pieces[Black][All] & ~cb.Attacks[White][All]))];

            return score;
        }

        public static int CalculatePositionScores(ChessBoard cb)
        {
            var score = 0;
            for (var color = White; color <= Black; color++)
            for (var pieceType = Pawn; pieceType <= King; pieceType++)
            {
                var piece = cb.Pieces[color][pieceType];
                while (piece != 0)
                {
                    score += EvalConstants.Psqt[pieceType][color][BitOperations.TrailingZeroCount(piece)];
                    piece &= piece - 1;
                }
            }

            return score;
        }

        public static int CalculateMaterialScore(ChessBoard cb)
        {
            return (BitOperations.PopCount((ulong) cb.Pieces[White][Pawn]) -
                    BitOperations.PopCount((ulong) cb.Pieces[Black][Pawn])) *
                   EvalConstants.Material[Pawn]
                   + (BitOperations.PopCount((ulong) cb.Pieces[White][Knight]) -
                      BitOperations.PopCount((ulong) cb.Pieces[Black][Knight])) *
                   EvalConstants.Material[Knight]
                   + (BitOperations.PopCount((ulong) cb.Pieces[White][Bishop]) -
                      BitOperations.PopCount((ulong) cb.Pieces[Black][Bishop])) *
                   EvalConstants.Material[Bishop]
                   + (BitOperations.PopCount((ulong) cb.Pieces[White][Rook]) -
                      BitOperations.PopCount((ulong) cb.Pieces[Black][Rook])) *
                   EvalConstants.Material[Rook]
                   + (BitOperations.PopCount((ulong) cb.Pieces[White][Queen]) -
                      BitOperations.PopCount((ulong) cb.Pieces[Black][Queen])) *
                   EvalConstants.Material[Queen];
        }
    }
}