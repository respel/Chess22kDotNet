using System;
using System.Numerics;
using Chess22kDotNet.Engine;
using Chess22kDotNet.Eval;
using Chess22kDotNet.Move;
using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet2
{
    public class ChessBoard
    {
        private readonly int[] _castlingAndEpHistory = new int[EngineConstants.MaxMoves];
        public readonly long[][] Attacks = Util.CreateJaggedArray<long[][]>(2, 7);

        public readonly long[] DoubleAttacks = new long[2];
        public readonly int[] KingAttackersFlag = new int[2];

        public readonly int[] KingIndex = new int[2];
        public readonly int[] PieceIndexes = new int[64];

        public readonly long[][] Pieces = Util.CreateJaggedArray<long[][]>(2, 7);
        public readonly long[] ZobristKeyHistory = new long[EngineConstants.MaxMoves];

        public long AllPieces, EmptySpaces;
        public int CastlingRights;
        public long CheckingPieces, PinnedPieces, DiscoveredPieces;
        public int ColorToMove, ColorToMoveInverse;
        public int EpIndex;
        public int MaterialKey;
        public long MoveCount;
        public int MoveCounter;
        public long PassedPawnsAndOutposts;
        public int Phase;
        public int PsqtScore;
        public long ZobristKey, PawnZobristKey;

        public override string ToString()
        {
            return ChessBoardUtil.ToString(this);
        }

        private void ChangeSideToMove()
        {
            ColorToMove = ColorToMoveInverse;
            ColorToMoveInverse = 1 - ColorToMove;
        }

        public bool IsDiscoveredMove(int fromIndex)
        {
            if (DiscoveredPieces == 0) return false;

            return (DiscoveredPieces & (1L << fromIndex)) != 0;
        }

        private void PushHistoryValues()
        {
            ZobristKeyHistory[MoveCounter] = ZobristKey;
            _castlingAndEpHistory[MoveCounter] = (CastlingRights << 10) | EpIndex;
            MoveCounter++;
        }

        private void PopHistoryValues()
        {
            MoveCounter--;
            ZobristKey = ZobristKeyHistory[MoveCounter];
            if (_castlingAndEpHistory[MoveCounter] == 0)
            {
                CastlingRights = 0;
                EpIndex = 0;
            }
            else
            {
                CastlingRights = Util.RightTripleShift(_castlingAndEpHistory[MoveCounter], 10);
                EpIndex = _castlingAndEpHistory[MoveCounter] & 255;
            }
        }

        public void DoNullMove()
        {
            PushHistoryValues();

            ZobristKey ^= Zobrist.SideToMove;
            if (EpIndex != 0)
            {
                ZobristKey ^= Zobrist.EpIndex[EpIndex];
                EpIndex = 0;
            }

            ChangeSideToMove();

            if (EngineConstants.Assert) ChessBoardTestUtil.TestValues(this);
        }

        public void UndoNullMove()
        {
            PopHistoryValues();
            ChangeSideToMove();

            if (EngineConstants.Assert) ChessBoardTestUtil.TestValues(this);
        }

        public void DoMove(int move)
        {
            MoveCount++;

            var fromIndex = MoveUtil.GetFromIndex(move);
            var toIndex = MoveUtil.GetToIndex(move);
            var toMask = 1L << toIndex;
            var fromToMask = (1L << fromIndex) ^ toMask;
            var sourcePieceIndex = MoveUtil.GetSourcePieceIndex(move);
            var attackedPieceIndex = MoveUtil.GetAttackedPieceIndex(move);

            if (EngineConstants.Assert)
            {
                Assert.IsTrue(move != 0);
                Assert.IsTrue(attackedPieceIndex != King);
                Assert.IsTrue(attackedPieceIndex == 0 || (Util.PowerLookup[toIndex] & Pieces[ColorToMove][All]) == 0);
                Assert.IsTrue(IsValidMove(move));
            }

            PushHistoryValues();

            ZobristKey ^= Zobrist.Piece[ColorToMove][sourcePieceIndex][fromIndex] ^
                          Zobrist.Piece[ColorToMove][sourcePieceIndex][toIndex] ^ Zobrist.SideToMove;
            if (EpIndex != 0)
            {
                ZobristKey ^= Zobrist.EpIndex[EpIndex];
                EpIndex = 0;
            }

            Pieces[ColorToMove][All] ^= fromToMask;
            Pieces[ColorToMove][sourcePieceIndex] ^= fromToMask;
            PieceIndexes[fromIndex] = Empty;
            PieceIndexes[toIndex] = sourcePieceIndex;
            PsqtScore += EvalConstants.Psqt[sourcePieceIndex][ColorToMove][toIndex] -
                         EvalConstants.Psqt[sourcePieceIndex][ColorToMove][fromIndex];

            switch (sourcePieceIndex)
            {
                case Pawn:
                    PawnZobristKey ^= Zobrist.Piece[ColorToMove][Pawn][fromIndex];
                    if (MoveUtil.IsPromotion(move))
                    {
                        Phase -= EvalConstants.Phase[MoveUtil.GetMoveType(move)];
                        MaterialKey += MaterialUtil.Values[ColorToMove][MoveUtil.GetMoveType(move)] -
                                       MaterialUtil.Values[ColorToMove][Pawn];
                        Pieces[ColorToMove][Pawn] ^= toMask;
                        Pieces[ColorToMove][MoveUtil.GetMoveType(move)] |= toMask;
                        PieceIndexes[toIndex] = MoveUtil.GetMoveType(move);
                        ZobristKey ^= Zobrist.Piece[ColorToMove][Pawn][toIndex] ^
                                      Zobrist.Piece[ColorToMove][MoveUtil.GetMoveType(move)][toIndex];
                        PsqtScore += EvalConstants.Psqt[MoveUtil.GetMoveType(move)][ColorToMove][toIndex] -
                                     EvalConstants.Psqt[Pawn][ColorToMove][toIndex];
                    }
                    else
                    {
                        PawnZobristKey ^= Zobrist.Piece[ColorToMove][Pawn][toIndex];
                        // 2-move
                        if (InBetween[fromIndex][toIndex] != 0)
                        {
                            if ((StaticMoves.PawnAttacks[ColorToMove][
                                     BitOperations.TrailingZeroCount(InBetween[fromIndex][toIndex])]
                                 & Pieces[ColorToMoveInverse][Pawn]) != 0)
                            {
                                EpIndex = BitOperations.TrailingZeroCount(InBetween[fromIndex][toIndex]);
                                ZobristKey ^= Zobrist.EpIndex[EpIndex];
                            }
                        }
                    }

                    break;

                case Rook:
                    if (CastlingRights != 0)
                    {
                        ZobristKey ^= Zobrist.Castling[CastlingRights];
                        CastlingRights = CastlingUtil.GetRookMovedOrAttackedCastlingRights(CastlingRights, fromIndex);
                        ZobristKey ^= Zobrist.Castling[CastlingRights];
                    }

                    break;

                case King:
                    KingIndex[ColorToMove] = toIndex;
                    if (CastlingRights != 0)
                    {
                        if (MoveUtil.IsCastlingMove(move)) CastlingUtil.CastleRookUpdateKeyAndPsqt(this, toIndex);

                        ZobristKey ^= Zobrist.Castling[CastlingRights];
                        CastlingRights = CastlingUtil.GetKingMovedCastlingRights(CastlingRights, fromIndex);
                        ZobristKey ^= Zobrist.Castling[CastlingRights];
                    }

                    break;
            }

            // piece hit?
            switch (attackedPieceIndex)
            {
                case Empty:
                    break;
                case Pawn:
                    if (MoveUtil.IsEpMove(move))
                    {
                        toIndex += ColorFactor8[ColorToMoveInverse];
                        toMask = Util.PowerLookup[toIndex];
                        PieceIndexes[toIndex] = Empty;
                    }

                    PawnZobristKey ^= Zobrist.Piece[ColorToMoveInverse][Pawn][toIndex];
                    PsqtScore -= EvalConstants.Psqt[Pawn][ColorToMoveInverse][toIndex];
                    Pieces[ColorToMoveInverse][All] ^= toMask;
                    Pieces[ColorToMoveInverse][Pawn] ^= toMask;
                    ZobristKey ^= Zobrist.Piece[ColorToMoveInverse][Pawn][toIndex];
                    MaterialKey -= MaterialUtil.Values[ColorToMoveInverse][Pawn];
                    break;
                case Rook:
                    if (CastlingRights != 0)
                    {
                        ZobristKey ^= Zobrist.Castling[CastlingRights];
                        CastlingRights = CastlingUtil.GetRookMovedOrAttackedCastlingRights(CastlingRights, toIndex);
                        ZobristKey ^= Zobrist.Castling[CastlingRights];
                    }

                    goto default;
                // fall-through
                default:
                    Phase += EvalConstants.Phase[attackedPieceIndex];
                    PsqtScore -= EvalConstants.Psqt[attackedPieceIndex][ColorToMoveInverse][toIndex];
                    Pieces[ColorToMoveInverse][All] ^= toMask;
                    Pieces[ColorToMoveInverse][attackedPieceIndex] ^= toMask;
                    ZobristKey ^= Zobrist.Piece[ColorToMoveInverse][attackedPieceIndex][toIndex];
                    MaterialKey -= MaterialUtil.Values[ColorToMoveInverse][attackedPieceIndex];
                    break;
            }

            AllPieces = Pieces[ColorToMove][All] | Pieces[ColorToMoveInverse][All];
            EmptySpaces = ~AllPieces;
            ChangeSideToMove();
            SetCheckingPinnedAndDiscoPieces();

            if (EngineConstants.Assert) ChessBoardTestUtil.TestValues(this);
        }

        public void SetCheckingPinnedAndDiscoPieces()
        {
            PinnedPieces = 0;
            DiscoveredPieces = 0;
            CheckingPieces = (Pieces[ColorToMoveInverse][Knight] & StaticMoves.KnightMoves[KingIndex[ColorToMove]])
                             | (Pieces[ColorToMoveInverse][Pawn] &
                                StaticMoves.PawnAttacks[ColorToMove][KingIndex[ColorToMove]]);

            for (var kingColor = White; kingColor <= Black; kingColor++)
            {
                var enemyColor = 1 - kingColor;

                if (!MaterialUtil.HasSlidingPieces(MaterialKey, enemyColor)) continue;

                var enemyPiece = ((Pieces[enemyColor][Bishop] | Pieces[enemyColor][Queen]) &
                                  MagicUtil.GetBishopMovesEmptyBoard(KingIndex[kingColor]))
                                 | ((Pieces[enemyColor][Rook] | Pieces[enemyColor][Queen]) &
                                    MagicUtil.GetRookMovesEmptyBoard(KingIndex[kingColor]));
                while (enemyPiece != 0)
                {
                    var checkedPiece = InBetween[KingIndex[kingColor]][BitOperations.TrailingZeroCount(enemyPiece)] &
                                       AllPieces;
                    if (checkedPiece == 0)
                    {
                        CheckingPieces |= enemyPiece & -enemyPiece;
                    }
                    else if (BitOperations.PopCount((ulong)checkedPiece) == 1)
                    {
                        PinnedPieces |= checkedPiece & Pieces[kingColor][All];
                        DiscoveredPieces |= checkedPiece & Pieces[enemyColor][All];
                    }

                    enemyPiece &= enemyPiece - 1;
                }
            }
        }

        public void UndoMove(int move)
        {
            var fromIndex = MoveUtil.GetFromIndex(move);
            var toIndex = MoveUtil.GetToIndex(move);
            var toMask = 1L << toIndex;
            var fromToMask = (1L << fromIndex) ^ toMask;
            var sourcePieceIndex = MoveUtil.GetSourcePieceIndex(move);
            var attackedPieceIndex = MoveUtil.GetAttackedPieceIndex(move);

            PopHistoryValues();

            // undo move
            Pieces[ColorToMoveInverse][All] ^= fromToMask;
            Pieces[ColorToMoveInverse][sourcePieceIndex] ^= fromToMask;
            PieceIndexes[fromIndex] = sourcePieceIndex;
            PsqtScore += EvalConstants.Psqt[sourcePieceIndex][ColorToMoveInverse][fromIndex] -
                         EvalConstants.Psqt[sourcePieceIndex][ColorToMoveInverse][toIndex];

            switch (sourcePieceIndex)
            {
                case Empty:
                    // not necessary but provides a table-index
                    break;
                case Pawn:
                    PawnZobristKey ^= Zobrist.Piece[ColorToMoveInverse][Pawn][fromIndex];
                    if (MoveUtil.IsPromotion(move))
                    {
                        Phase += EvalConstants.Phase[MoveUtil.GetMoveType(move)];
                        MaterialKey -= MaterialUtil.Values[ColorToMoveInverse][MoveUtil.GetMoveType(move)] -
                                       MaterialUtil.Values[ColorToMoveInverse][Pawn];
                        Pieces[ColorToMoveInverse][Pawn] ^= toMask;
                        Pieces[ColorToMoveInverse][MoveUtil.GetMoveType(move)] ^= toMask;
                        PsqtScore += EvalConstants.Psqt[Pawn][ColorToMoveInverse][toIndex]
                                     - EvalConstants.Psqt[MoveUtil.GetMoveType(move)][ColorToMoveInverse][toIndex];
                    }
                    else
                    {
                        PawnZobristKey ^= Zobrist.Piece[ColorToMoveInverse][Pawn][toIndex];
                    }

                    break;
                case King:
                    if (MoveUtil.IsCastlingMove(move)) CastlingUtil.UncastleRookUpdatePsqt(this, toIndex);

                    KingIndex[ColorToMoveInverse] = fromIndex;
                    break;
            }

            // undo hit
            switch (attackedPieceIndex)
            {
                case Empty:
                    break;
                case Pawn:
                    if (MoveUtil.IsEpMove(move))
                    {
                        PieceIndexes[toIndex] = Empty;
                        toIndex += ColorFactor8[ColorToMove];
                        toMask = Util.PowerLookup[toIndex];
                    }

                    PawnZobristKey ^= Zobrist.Piece[ColorToMove][Pawn][toIndex];
                    goto default;
                // fall-through
                default:
                    PsqtScore += EvalConstants.Psqt[attackedPieceIndex][ColorToMove][toIndex];
                    Phase -= EvalConstants.Phase[attackedPieceIndex];
                    MaterialKey += MaterialUtil.Values[ColorToMove][attackedPieceIndex];
                    Pieces[ColorToMove][All] |= toMask;
                    Pieces[ColorToMove][attackedPieceIndex] |= toMask;
                    break;
            }

            PieceIndexes[toIndex] = attackedPieceIndex;
            AllPieces = Pieces[ColorToMove][All] | Pieces[ColorToMoveInverse][All];
            EmptySpaces = ~AllPieces;
            ChangeSideToMove();
            SetCheckingPinnedAndDiscoPieces();

            if (EngineConstants.Assert) ChessBoardTestUtil.TestValues(this);
        }

        public bool IsLegal(int move)
        {
            return MoveUtil.GetSourcePieceIndex(move) != King || IsLegalKingMove(move);
        }

        private bool IsLegalKingMove(int move)
        {
            return !CheckUtil.IsInCheckIncludingKing(MoveUtil.GetToIndex(move), ColorToMove, Pieces[ColorToMoveInverse],
                AllPieces ^ Util.PowerLookup[MoveUtil.GetFromIndex(move)]);
        }

        private bool IsLegalNonKingMove(int move)
        {
            return !CheckUtil.IsInCheck(KingIndex[ColorToMove], ColorToMove, Pieces[ColorToMoveInverse],
                AllPieces ^ Util.PowerLookup[MoveUtil.GetFromIndex(move)] ^
                Util.PowerLookup[MoveUtil.GetToIndex(move)]);
        }

        public bool IsLegalEpMove(int fromIndex)
        {
            if (EpIndex == 0)
                // required for tt-moves
                return false;

            // do-move and hit
            Pieces[ColorToMoveInverse][Pawn] ^= Util.PowerLookup[EpIndex + ColorFactor8[ColorToMoveInverse]];

            // check if is in check
            var isInCheck = CheckUtil.IsInCheck(KingIndex[ColorToMove], ColorToMove, Pieces[ColorToMoveInverse],
                (Pieces[ColorToMove][All] ^ Util.PowerLookup[fromIndex] ^ Util.PowerLookup[EpIndex])
                | (Pieces[ColorToMoveInverse][All] ^ Util.PowerLookup[EpIndex + ColorFactor8[ColorToMoveInverse]]));

            // undo-move and hit
            Pieces[ColorToMoveInverse][Pawn] |= Util.PowerLookup[EpIndex + ColorFactor8[ColorToMoveInverse]];

            return !isInCheck;
        }

        public bool IsValidMove(int move)
        {
            // check piece at from square
            var fromIndex = MoveUtil.GetFromIndex(move);
            var fromSquare = Util.PowerLookup[fromIndex];
            if ((Pieces[ColorToMove][MoveUtil.GetSourcePieceIndex(move)] & fromSquare) == 0) return false;

            // check piece at to square
            var toIndex = MoveUtil.GetToIndex(move);
            var toSquare = Util.PowerLookup[toIndex];
            var attackedPieceIndex = MoveUtil.GetAttackedPieceIndex(move);
            if (attackedPieceIndex == 0)
            {
                if (PieceIndexes[toIndex] != Empty) return false;
            }
            else
            {
                if ((Pieces[ColorToMoveInverse][attackedPieceIndex] & toSquare) == 0 && !MoveUtil.IsEpMove(move))
                    return false;
            }

            // check if move is possible
            switch (MoveUtil.GetSourcePieceIndex(move))
            {
                case Pawn:
                    if (MoveUtil.IsEpMove(move))
                    {
                        return toIndex == EpIndex && IsLegalEpMove(fromIndex);
                    }
                    else
                    {
                        if (ColorToMove == White)
                        {
                            if (fromIndex > toIndex) return false;

                            // 2-move
                            if (toIndex - fromIndex == 16 && (AllPieces & Util.PowerLookup[fromIndex + 8]) != 0)
                                return false;
                        }
                        else
                        {
                            if (fromIndex < toIndex) return false;

                            // 2-move
                            if (fromIndex - toIndex == 16 && (AllPieces & Util.PowerLookup[fromIndex - 8]) != 0)
                                return false;
                        }
                    }

                    break;
                case Knight:
                    break;
                case Bishop:
                // fall-through
                case Rook:
                // fall-through
                case Queen:
                    if ((InBetween[fromIndex][toIndex] & AllPieces) != 0) return false;

                    break;
                case King:
                    if (!MoveUtil.IsCastlingMove(move)) return IsLegalKingMove(move);
                    var castlingIndexes = CastlingUtil.GetCastlingIndexes(this);
                    while (castlingIndexes != 0)
                    {
                        if (toIndex == BitOperations.TrailingZeroCount(castlingIndexes))
                            return CastlingUtil.IsValidCastlingMove(this, fromIndex, toIndex);

                        castlingIndexes &= castlingIndexes - 1;
                    }

                    return false;
            }

            if ((fromSquare & PinnedPieces) != 0)
            {
                if ((PinnedMovement[fromIndex][KingIndex[ColorToMove]] & toSquare) == 0)
                    return false;
            }

            if (CheckingPieces == 0) return true;
            if (attackedPieceIndex == 0) return IsLegalNonKingMove(move);

            if (BitOperations.PopCount((ulong)CheckingPieces) == 2) return false;

            return (toSquare & CheckingPieces) != 0;
        }

        public bool IsRepetition(int move)
        {
            if (!EngineConstants.EnableRepetitionTable) return false;

            // if move was an attacking-move or pawn move, no repetition
            if (!MoveUtil.IsQuiet(move) || MoveUtil.GetSourcePieceIndex(move) == Pawn) return false;

            var moveCountMin = Math.Max(0, MoveCounter - 50);
            for (var i = MoveCounter - 2; i >= moveCountMin; i -= 2)
            {
                if (ZobristKey != ZobristKeyHistory[i]) continue;
                if (Statistics.Enabled) Statistics.Repetitions++;

                return true;
            }

            return false;
        }

        public void ClearEvalAttacks()
        {
            KingAttackersFlag[White] = 0;
            KingAttackersFlag[Black] = 0;
            Attacks[White][All] = 0;
            Attacks[White][Pawn] = 0;
            Attacks[White][Knight] = 0;
            Attacks[White][Bishop] = 0;
            Attacks[White][Rook] = 0;
            Attacks[White][Queen] = 0;
            Attacks[Black][All] = 0;
            Attacks[Black][Pawn] = 0;
            Attacks[Black][Knight] = 0;
            Attacks[Black][Bishop] = 0;
            Attacks[Black][Rook] = 0;
            Attacks[Black][Queen] = 0;
            DoubleAttacks[White] = 0;
            DoubleAttacks[Black] = 0;
        }

        public void UpdateAttacks(long moves, int piece, int color, long kingArea)
        {
            if ((moves & kingArea) != 0) KingAttackersFlag[color] |= SchroderUtil.Flags[piece];

            DoubleAttacks[color] |= Attacks[color][All] & moves;
            Attacks[color][All] |= moves;
            Attacks[color][piece] |= moves;
        }

        public void UpdatePawnAttacks(long moves, int color)
        {
            DoubleAttacks[color] |= Attacks[color][Pawn] & moves;
            Attacks[color][Pawn] |= moves;
        }

        public void UpdatePawnAttacks(int color, long kingArea)
        {
            Attacks[color][All] = Attacks[color][Pawn];
            if ((Attacks[color][Pawn] & kingArea) != 0) KingAttackersFlag[color] |= SchroderUtil.Flags[Pawn];
        }
    }
}