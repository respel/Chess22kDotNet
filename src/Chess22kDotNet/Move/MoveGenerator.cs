using System.Collections.Generic;
using System.Numerics;
using Chess22kDotNet.Engine;
using Chess22kDotNet.Search;
using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet.Move
{
    public static class MoveGenerator
    {
        public static void GenerateMoves(ThreadData threadData, ChessBoard cb)
        {
            if (cb.CheckingPieces == 0)
            {
                GenerateNotInCheckMoves(threadData, cb);
            }
            else if (BitOperations.PopCount((ulong) cb.CheckingPieces) == 1)
            {
                GenerateOutOfCheckMoves(threadData, cb);
            }
            else
            {
                // double check, only the king can move
                AddKingMoves(threadData, cb);
            }
        }

        public static void GenerateAttacks(ThreadData threadData, ChessBoard cb)
        {
            if (cb.CheckingPieces == 0)
            {
                GenerateNotInCheckAttacks(threadData, cb);
            }
            else if (BitOperations.PopCount((ulong) cb.CheckingPieces) == 1)
            {
                GenerateOutOfCheckAttacks(threadData, cb);
            }
            else
            {
                // double check, only the king can attack
                AddKingAttacks(threadData, cb);
            }
        }

        private static void GenerateNotInCheckMoves(ThreadData threadData, ChessBoard cb)
        {
            // non pinned pieces
            var nonPinned = ~cb.PinnedPieces;
            var pieces = cb.Pieces[cb.ColorToMove];
            AddNightMoves(threadData, pieces[Knight] & nonPinned, cb.EmptySpaces);
            AddBishopMoves(threadData, pieces[Bishop] & nonPinned, cb.AllPieces, cb.EmptySpaces);
            AddRookMoves(threadData, pieces[Rook] & nonPinned, cb.AllPieces, cb.EmptySpaces);
            AddQueenMoves(threadData, pieces[Queen] & nonPinned, cb.AllPieces, cb.EmptySpaces);
            AddPawnMoves(threadData, pieces[Pawn] & nonPinned, cb, cb.EmptySpaces);
            AddKingMoves(threadData, cb);

            // pinned pieces
            var piece = cb.Pieces[cb.ColorToMove][All] & cb.PinnedPieces;
            while (piece != 0)
            {
                switch (cb.PieceIndexes[BitOperations.TrailingZeroCount(piece)])
                {
                    case Pawn:
                        AddPawnMoves(threadData, piece & -piece, cb,
                            cb.EmptySpaces &
                            PinnedMovement[BitOperations.TrailingZeroCount(piece)][cb.KingIndex[cb.ColorToMove]]);
                        break;
                    case Bishop:
                        AddBishopMoves(threadData, piece & -piece, cb.AllPieces,
                            cb.EmptySpaces &
                            PinnedMovement[BitOperations.TrailingZeroCount(piece)][cb.KingIndex[cb.ColorToMove]]);
                        break;
                    case Rook:
                        AddRookMoves(threadData, piece & -piece, cb.AllPieces,
                            cb.EmptySpaces &
                            PinnedMovement[BitOperations.TrailingZeroCount(piece)][cb.KingIndex[cb.ColorToMove]]);
                        break;
                    case Queen:
                        AddQueenMoves(threadData, piece & -piece, cb.AllPieces,
                            cb.EmptySpaces &
                            PinnedMovement[BitOperations.TrailingZeroCount(piece)][cb.KingIndex[cb.ColorToMove]]);
                        break;
                }

                piece &= piece - 1;
            }
        }

        private static void GenerateOutOfCheckMoves(ThreadData threadData, ChessBoard cb)
        {
            var inBetween = InBetween[cb.KingIndex[cb.ColorToMove]][BitOperations.TrailingZeroCount(cb.CheckingPieces)];
            if (inBetween != 0)
            {
                var nonPinned = ~cb.PinnedPieces;
                var pieces = cb.Pieces[cb.ColorToMove];
                AddPawnMoves(threadData, pieces[Pawn] & nonPinned, cb, inBetween);
                AddNightMoves(threadData, pieces[Knight] & nonPinned, inBetween);
                AddBishopMoves(threadData, pieces[Bishop] & nonPinned, cb.AllPieces, inBetween);
                AddRookMoves(threadData, pieces[Rook] & nonPinned, cb.AllPieces, inBetween);
                AddQueenMoves(threadData, pieces[Queen] & nonPinned, cb.AllPieces, inBetween);
            }

            AddKingMoves(threadData, cb);
        }

        private static void GenerateNotInCheckAttacks(ThreadData threadData, ChessBoard cb)
        {
            // non pinned pieces
            AddEpAttacks(threadData, cb);
            var nonPinned = ~cb.PinnedPieces;
            var enemies = cb.Pieces[cb.ColorToMoveInverse][All];
            var pieces = cb.Pieces[cb.ColorToMove];
            AddPawnAttacksAndPromotions(threadData, pieces[Pawn] & nonPinned, cb, enemies, cb.EmptySpaces);
            AddKnightAttacks(threadData, pieces[Knight] & nonPinned, cb.PieceIndexes, enemies);
            AddBishopAttacks(threadData, pieces[Bishop] & nonPinned, cb, enemies);
            AddRookAttacks(threadData, pieces[Rook] & nonPinned, cb, enemies);
            AddQueenAttacks(threadData, pieces[Queen] & nonPinned, cb, enemies);
            AddKingAttacks(threadData, cb);

            // pinned pieces
            var piece = cb.Pieces[cb.ColorToMove][All] & cb.PinnedPieces;
            while (piece != 0)
            {
                switch (cb.PieceIndexes[BitOperations.TrailingZeroCount(piece)])
                {
                    case Pawn:
                        AddPawnAttacksAndPromotions(threadData, piece & -piece, cb,
                            enemies & PinnedMovement[BitOperations.TrailingZeroCount(piece)][
                                cb.KingIndex[cb.ColorToMove]], 0);
                        break;
                    case Bishop:
                        AddBishopAttacks(threadData, piece & -piece, cb,
                            enemies & PinnedMovement[BitOperations.TrailingZeroCount(piece)][
                                cb.KingIndex[cb.ColorToMove]]);
                        break;
                    case Rook:
                        AddRookAttacks(threadData, piece & -piece, cb,
                            enemies & PinnedMovement[BitOperations.TrailingZeroCount(piece)][
                                cb.KingIndex[cb.ColorToMove]]);
                        break;
                    case Queen:
                        AddQueenAttacks(threadData, piece & -piece, cb,
                            enemies & PinnedMovement[BitOperations.TrailingZeroCount(piece)][
                                cb.KingIndex[cb.ColorToMove]]);
                        break;
                }

                piece &= piece - 1;
            }
        }

        private static void GenerateOutOfCheckAttacks(ThreadData threadData, ChessBoard cb)
        {
            // attack attacker
            var nonPinned = ~cb.PinnedPieces;
            var pieces = cb.Pieces[cb.ColorToMove];
            AddEpAttacks(threadData, cb);
            AddPawnAttacksAndPromotions(threadData, pieces[Pawn] & nonPinned, cb, cb.CheckingPieces,
                InBetween[cb.KingIndex[cb.ColorToMove]][BitOperations.TrailingZeroCount(cb.CheckingPieces)]);
            AddKnightAttacks(threadData, pieces[Knight] & nonPinned, cb.PieceIndexes, cb.CheckingPieces);
            AddBishopAttacks(threadData, pieces[Bishop] & nonPinned, cb, cb.CheckingPieces);
            AddRookAttacks(threadData, pieces[Rook] & nonPinned, cb, cb.CheckingPieces);
            AddQueenAttacks(threadData, pieces[Queen] & nonPinned, cb, cb.CheckingPieces);
            AddKingAttacks(threadData, cb);
        }

        private static void AddPawnAttacksAndPromotions(ThreadData threadData, long pawns, ChessBoard cb,
            in long enemies,
            in long emptySpaces)
        {
            if (pawns == 0)
            {
                return;
            }

            if (cb.ColorToMove == White)
            {
                // non-promoting
                var piece = pawns & Bitboard.RankNonPromotion[White] & Bitboard.GetBlackPawnAttacks(enemies);
                while (piece != 0)
                {
                    var fromIndex = BitOperations.TrailingZeroCount(piece);
                    var moves = StaticMoves.PawnAttacks[White][fromIndex] & enemies;
                    while (moves != 0)
                    {
                        var toIndex = BitOperations.TrailingZeroCount(moves);
                        threadData.AddMove(
                            MoveUtil.CreateAttackMove(fromIndex, toIndex, Pawn, cb.PieceIndexes[toIndex]));
                        moves &= moves - 1;
                    }

                    piece &= piece - 1;
                }

                // promoting
                piece = pawns & Bitboard.Rank7;
                while (piece != 0)
                {
                    var fromIndex = BitOperations.TrailingZeroCount(piece);

                    // promotion move
                    if (((piece & -piece) << 8 & emptySpaces) != 0)
                    {
                        AddPromotionMove(threadData, fromIndex, fromIndex + 8);
                    }

                    // promotion attacks
                    AddPromotionAttacks(threadData, StaticMoves.PawnAttacks[White][fromIndex] & enemies, fromIndex,
                        cb.PieceIndexes);

                    piece &= piece - 1;
                }
            }
            else
            {
                // non-promoting
                var piece = pawns & Bitboard.RankNonPromotion[Black] & Bitboard.GetWhitePawnAttacks(enemies);
                while (piece != 0)
                {
                    var fromIndex = BitOperations.TrailingZeroCount(piece);
                    var moves = StaticMoves.PawnAttacks[Black][fromIndex] & enemies;
                    while (moves != 0)
                    {
                        var toIndex = BitOperations.TrailingZeroCount(moves);
                        threadData.AddMove(
                            MoveUtil.CreateAttackMove(fromIndex, toIndex, Pawn, cb.PieceIndexes[toIndex]));
                        moves &= moves - 1;
                    }

                    piece &= piece - 1;
                }

                // promoting
                piece = pawns & Bitboard.Rank2;
                while (piece != 0)
                {
                    var fromIndex = BitOperations.TrailingZeroCount(piece);

                    // promotion move
                    if ((Util.RightTripleShift(piece & -piece, 8) & emptySpaces) != 0)
                    {
                        AddPromotionMove(threadData, fromIndex, fromIndex - 8);
                    }

                    // promotion attacks
                    AddPromotionAttacks(threadData, StaticMoves.PawnAttacks[Black][fromIndex] & enemies, fromIndex,
                        cb.PieceIndexes);

                    piece &= piece - 1;
                }
            }
        }

        private static void AddBishopAttacks(ThreadData threadData, long piece, ChessBoard cb,
            long possiblePositions)
        {
            while (piece != 0)
            {
                var fromIndex = BitOperations.TrailingZeroCount(piece);
                var moves = MagicUtil.GetBishopMoves(fromIndex, cb.AllPieces) & possiblePositions;
                while (moves != 0)
                {
                    var toIndex = BitOperations.TrailingZeroCount(moves);
                    threadData.AddMove(MoveUtil.CreateAttackMove(fromIndex, toIndex, Bishop, cb.PieceIndexes[toIndex]));
                    moves &= moves - 1;
                }

                piece &= piece - 1;
            }
        }

        private static void AddRookAttacks(ThreadData threadData, long piece, ChessBoard cb,
            long possiblePositions)
        {
            while (piece != 0)
            {
                var fromIndex = BitOperations.TrailingZeroCount(piece);
                var moves = MagicUtil.GetRookMoves(fromIndex, cb.AllPieces) & possiblePositions;
                while (moves != 0)
                {
                    var toIndex = BitOperations.TrailingZeroCount(moves);
                    threadData.AddMove(MoveUtil.CreateAttackMove(fromIndex, toIndex, Rook, cb.PieceIndexes[toIndex]));
                    moves &= moves - 1;
                }

                piece &= piece - 1;
            }
        }

        private static void AddQueenAttacks(ThreadData threadData, long piece, ChessBoard cb,
            long possiblePositions)
        {
            while (piece != 0)
            {
                var fromIndex = BitOperations.TrailingZeroCount(piece);
                var moves = MagicUtil.GetQueenMoves(fromIndex, cb.AllPieces) & possiblePositions;
                while (moves != 0)
                {
                    var toIndex = BitOperations.TrailingZeroCount(moves);
                    threadData.AddMove(MoveUtil.CreateAttackMove(fromIndex, toIndex, Queen, cb.PieceIndexes[toIndex]));
                    moves &= moves - 1;
                }

                piece &= piece - 1;
            }
        }

        private static void AddBishopMoves(ThreadData threadData, long piece, long allPieces,
            long possiblePositions)
        {
            while (piece != 0)
            {
                var fromIndex = BitOperations.TrailingZeroCount(piece);
                var moves = MagicUtil.GetBishopMoves(fromIndex, allPieces) & possiblePositions;
                while (moves != 0)
                {
                    threadData.AddMove(MoveUtil.CreateMove(fromIndex, BitOperations.TrailingZeroCount(moves), Bishop));
                    moves &= moves - 1;
                }

                piece &= piece - 1;
            }
        }

        private static void AddQueenMoves(ThreadData threadData, long piece, long allPieces,
            long possiblePositions)
        {
            while (piece != 0)
            {
                var fromIndex = BitOperations.TrailingZeroCount(piece);
                var moves = MagicUtil.GetQueenMoves(fromIndex, allPieces) & possiblePositions;
                while (moves != 0)
                {
                    threadData.AddMove(MoveUtil.CreateMove(fromIndex, BitOperations.TrailingZeroCount(moves), Queen));
                    moves &= moves - 1;
                }

                piece &= piece - 1;
            }
        }

        private static void AddRookMoves(ThreadData threadData, long piece, long allPieces,
            long possiblePositions)
        {
            while (piece != 0)
            {
                var fromIndex = BitOperations.TrailingZeroCount(piece);
                var moves = MagicUtil.GetRookMoves(fromIndex, allPieces) & possiblePositions;
                while (moves != 0)
                {
                    threadData.AddMove(MoveUtil.CreateMove(fromIndex, BitOperations.TrailingZeroCount(moves), Rook));
                    moves &= moves - 1;
                }

                piece &= piece - 1;
            }
        }

        private static void AddNightMoves(ThreadData threadData, long piece, long possiblePositions)
        {
            while (piece != 0)
            {
                var fromIndex = BitOperations.TrailingZeroCount(piece);
                var moves = StaticMoves.KnightMoves[fromIndex] & possiblePositions;
                while (moves != 0)
                {
                    threadData.AddMove(MoveUtil.CreateMove(fromIndex, BitOperations.TrailingZeroCount(moves), Knight));
                    moves &= moves - 1;
                }

                piece &= piece - 1;
            }
        }

        private static void AddPawnMoves(ThreadData threadData, long pawns, ChessBoard cb,
            long possiblePositions)
        {
            if (pawns == 0)
            {
                return;
            }

            if (cb.ColorToMove == White)
            {
                // 1-move
                var piece = pawns & Util.RightTripleShift(possiblePositions, 8) & Bitboard.Rank23456;
                while (piece != 0)
                {
                    threadData.AddMove(MoveUtil.CreateWhitePawnMove(BitOperations.TrailingZeroCount(piece)));
                    piece &= piece - 1;
                }

                // 2-move
                piece = pawns & Util.RightTripleShift(possiblePositions, 16) & Bitboard.Rank2;
                while (piece != 0)
                {
                    if ((cb.EmptySpaces & ((piece & -piece) << 8)) != 0)
                    {
                        threadData.AddMove(MoveUtil.CreateWhitePawn2Move(BitOperations.TrailingZeroCount(piece)));
                    }

                    piece &= piece - 1;
                }
            }
            else
            {
                // 1-move
                var piece = pawns & (possiblePositions << 8) & Bitboard.Rank34567;
                while (piece != 0)
                {
                    threadData.AddMove(MoveUtil.CreateBlackPawnMove(BitOperations.TrailingZeroCount(piece)));
                    piece &= piece - 1;
                }

                // 2-move
                piece = pawns & (possiblePositions << 16) & Bitboard.Rank7;
                while (piece != 0)
                {
                    if ((cb.EmptySpaces & Util.RightTripleShift(piece & -piece, 8)) != 0)
                    {
                        threadData.AddMove(MoveUtil.CreateBlackPawn2Move(BitOperations.TrailingZeroCount(piece)));
                    }

                    piece &= piece - 1;
                }
            }
        }

        private static void AddKingMoves(ThreadData threadData, ChessBoard cb)
        {
            var fromIndex = cb.KingIndex[cb.ColorToMove];
            var moves = StaticMoves.KingMoves[fromIndex] & cb.EmptySpaces;
            while (moves != 0)
            {
                threadData.AddMove(MoveUtil.CreateMove(fromIndex, BitOperations.TrailingZeroCount(moves), King));
                moves &= moves - 1;
            }

            // castling
            if (cb.CheckingPieces != 0) return;
            var castlingIndexes = CastlingUtil.GetCastlingIndexes(cb);
            while (castlingIndexes != 0)
            {
                var castlingIndex = BitOperations.TrailingZeroCount(castlingIndexes);
                // no piece in between?
                if (CastlingUtil.IsValidCastlingMove(cb, fromIndex, castlingIndex))
                {
                    threadData.AddMove(MoveUtil.CreateCastlingMove(fromIndex, castlingIndex));
                }

                castlingIndexes &= castlingIndexes - 1;
            }
        }

        private static void AddKingAttacks(ThreadData threadData, ChessBoard cb)
        {
            var fromIndex = cb.KingIndex[cb.ColorToMove];
            var moves = StaticMoves.KingMoves[fromIndex] & cb.Pieces[cb.ColorToMoveInverse][All] & ~cb.DiscoveredPieces;
            while (moves != 0)
            {
                var toIndex = BitOperations.TrailingZeroCount(moves);
                threadData.AddMove(MoveUtil.CreateAttackMove(fromIndex, toIndex, King, cb.PieceIndexes[toIndex]));
                moves &= moves - 1;
            }
        }

        private static void AddKnightAttacks(ThreadData threadData, long piece, IReadOnlyList<int> pieceIndexes,
            long possiblePositions)
        {
            while (piece != 0)
            {
                var fromIndex = BitOperations.TrailingZeroCount(piece);
                var moves = StaticMoves.KnightMoves[fromIndex] & possiblePositions;
                while (moves != 0)
                {
                    var toIndex = BitOperations.TrailingZeroCount(moves);
                    threadData.AddMove(MoveUtil.CreateAttackMove(fromIndex, toIndex, Knight, pieceIndexes[toIndex]));
                    moves &= moves - 1;
                }

                piece &= piece - 1;
            }
        }

        private static void AddEpAttacks(ThreadData threadData, ChessBoard cb)
        {
            if (cb.EpIndex == 0)
            {
                return;
            }

            var piece = cb.Pieces[cb.ColorToMove][Pawn] & StaticMoves.PawnAttacks[cb.ColorToMoveInverse][cb.EpIndex];
            while (piece != 0)
            {
                if (cb.IsLegalEpMove(BitOperations.TrailingZeroCount(piece)))
                {
                    threadData.AddMove(MoveUtil.CreateEpMove(BitOperations.TrailingZeroCount(piece), cb.EpIndex));
                }

                piece &= piece - 1;
            }
        }

        private static void AddPromotionMove(ThreadData threadData, int fromIndex, int toIndex)
        {
            threadData.AddMove(MoveUtil.CreatePromotionMove(MoveUtil.TypePromotionQ, fromIndex, toIndex));
            threadData.AddMove(MoveUtil.CreatePromotionMove(MoveUtil.TypePromotionN, fromIndex, toIndex));
            if (!EngineConstants.GenerateBrPromotions) return;
            threadData.AddMove(MoveUtil.CreatePromotionMove(MoveUtil.TypePromotionB, fromIndex, toIndex));
            threadData.AddMove(MoveUtil.CreatePromotionMove(MoveUtil.TypePromotionR, fromIndex, toIndex));
        }

        private static void AddPromotionAttacks(ThreadData threadData, long moves, int fromIndex,
            IReadOnlyList<int> pieceIndexes)
        {
            while (moves != 0)
            {
                var toIndex = BitOperations.TrailingZeroCount(moves);
                threadData.AddMove(MoveUtil.CreatePromotionAttack(MoveUtil.TypePromotionQ, fromIndex, toIndex,
                    pieceIndexes[toIndex]));
                threadData.AddMove(MoveUtil.CreatePromotionAttack(MoveUtil.TypePromotionN, fromIndex, toIndex,
                    pieceIndexes[toIndex]));
                if (EngineConstants.GenerateBrPromotions)
                {
                    threadData.AddMove(MoveUtil.CreatePromotionAttack(MoveUtil.TypePromotionB, fromIndex, toIndex,
                        pieceIndexes[toIndex]));
                    threadData.AddMove(MoveUtil.CreatePromotionAttack(MoveUtil.TypePromotionR, fromIndex, toIndex,
                        pieceIndexes[toIndex]));
                }

                moves &= moves - 1;
            }
        }
    }
}