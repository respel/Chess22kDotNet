using System;

namespace Chess22kDotNet.Move
{
    public class MoveWrapper
    {
        public readonly int FromRank;
        public readonly char FromFile;

        /** 1 to 8 */
        public readonly int ToRank;

        /** a to h */
        public readonly char ToFile;

        public readonly int FromIndex;
        public readonly int ToIndex;
        public readonly int Move;

        public readonly int PieceIndex;
        public readonly int PieceIndexAttacked;

        public readonly bool IsKnightPromotion;
        public readonly bool IsQueenPromotion;
        public readonly bool IsRookPromotion;
        public readonly bool IsBishopPromotion;

        public bool IsEp = false;
        public bool IsCastling = false;

        public MoveWrapper(int move)
        {
            Move = move;

            FromIndex = MoveUtil.GetFromIndex(move);
            FromFile = (char) (104 - FromIndex % 8);
            FromRank = FromIndex / 8 + 1;

            ToIndex = MoveUtil.GetToIndex(move);
            ToFile = (char) (104 - ToIndex % 8);
            ToRank = ToIndex / 8 + 1;

            PieceIndex = MoveUtil.GetSourcePieceIndex(move);
            PieceIndexAttacked = MoveUtil.GetAttackedPieceIndex(move);

            switch (MoveUtil.GetMoveType(move))
            {
                case MoveUtil.TypeNormal:
                    break;
                case MoveUtil.TypeCastling:
                    IsCastling = true;
                    break;
                case MoveUtil.TypeEp:
                    IsEp = true;
                    break;
                case MoveUtil.TypePromotionB:
                    IsBishopPromotion = true;
                    break;
                case MoveUtil.TypePromotionN:
                    IsKnightPromotion = true;
                    break;
                case MoveUtil.TypePromotionQ:
                    IsQueenPromotion = true;
                    break;
                case MoveUtil.TypePromotionR:
                    IsRookPromotion = true;
                    break;
                default:
                    throw new ArgumentException("Unknown movetype: " + MoveUtil.GetMoveType(move));
            }
        }

        public MoveWrapper(string moveString, ChessBoard cb)
        {
            FromFile = moveString[0];
            FromRank = int.Parse(moveString.Substring(1, 1));
            FromIndex = (FromRank - 1) * 8 + 104 - FromFile;

            ToFile = moveString[2];
            ToRank = int.Parse(moveString.Substring(3, 1));
            ToIndex = (ToRank - 1) * 8 + 104 - ToFile;

            //@formatter:off
            PieceIndex =
                (cb.Pieces[cb.ColorToMove][ChessConstants.Pawn] & Util.PowerLookup[FromIndex]) != 0
                    ? ChessConstants.Pawn
                    : (cb.Pieces[cb.ColorToMove][ChessConstants.Bishop] & Util.PowerLookup[FromIndex]) != 0
                        ? ChessConstants.Bishop
                        : (cb.Pieces[cb.ColorToMove][ChessConstants.Knight] & Util.PowerLookup[FromIndex]) != 0
                            ? ChessConstants.Knight
                            : (cb.Pieces[cb.ColorToMove][ChessConstants.King] & Util.PowerLookup[FromIndex]) != 0
                                ? ChessConstants.King
                                : (cb.Pieces[cb.ColorToMove][ChessConstants.Queen] & Util.PowerLookup[FromIndex]) != 0
                                    ? ChessConstants.Queen
                                    : (cb.Pieces[cb.ColorToMove][ChessConstants.Rook] & Util.PowerLookup[FromIndex]) !=
                                      0
                                        ? ChessConstants.Rook
                                        : -1;
            if (PieceIndex == -1)
            {
                throw new ArgumentException("Source piece not found at index " + FromIndex);
            }

            PieceIndexAttacked =
                (cb.Pieces[cb.ColorToMoveInverse][ChessConstants.Pawn] & Util.PowerLookup[ToIndex]) != 0
                    ? ChessConstants.Pawn
                    : (cb.Pieces[cb.ColorToMoveInverse][ChessConstants.Bishop] & Util.PowerLookup[ToIndex]) != 0
                        ? ChessConstants.Bishop
                        : (cb.Pieces[cb.ColorToMoveInverse][ChessConstants.Knight] & Util.PowerLookup[ToIndex]) != 0
                            ? ChessConstants.Knight
                            : (cb.Pieces[cb.ColorToMoveInverse][ChessConstants.King] & Util.PowerLookup[ToIndex]) != 0
                                ? ChessConstants.King
                                : (cb.Pieces[cb.ColorToMoveInverse][ChessConstants.Queen] &
                                   Util.PowerLookup[ToIndex]) != 0
                                    ? ChessConstants.Queen
                                    : (cb.Pieces[cb.ColorToMoveInverse][ChessConstants.Rook] &
                                       Util.PowerLookup[ToIndex]) != 0
                                        ? ChessConstants.Rook
                                        : 0;
            //@formatter:on

            if (PieceIndexAttacked == 0)
            {
                switch (PieceIndex)
                {
                    case ChessConstants.Pawn when ToRank == 1 || ToRank == 8:
                    {
                        if (moveString.Length == 5)
                        {
                            switch (moveString.Substring(4, 1))
                            {
                                case "n":
                                    IsKnightPromotion = true;
                                    Move = MoveUtil.CreatePromotionMove(MoveUtil.TypePromotionN, FromIndex, ToIndex);
                                    break;
                                case "r":
                                    IsRookPromotion = true;
                                    Move = MoveUtil.CreatePromotionMove(MoveUtil.TypePromotionR, FromIndex, ToIndex);
                                    break;
                                case "b":
                                    IsBishopPromotion = true;
                                    Move = MoveUtil.CreatePromotionMove(MoveUtil.TypePromotionB, FromIndex, ToIndex);
                                    break;
                                case "q":
                                    IsQueenPromotion = true;
                                    Move = MoveUtil.CreatePromotionMove(MoveUtil.TypePromotionQ, FromIndex, ToIndex);
                                    break;
                            }
                        }
                        else
                        {
                            IsQueenPromotion = true;
                            Move = MoveUtil.CreatePromotionMove(MoveUtil.TypePromotionQ, FromIndex, ToIndex);
                        }

                        break;
                    }
                    case ChessConstants.King when FromIndex - ToIndex == 2 || FromIndex - ToIndex == -2:
                        // castling
                        Move = MoveUtil.CreateCastlingMove(FromIndex, ToIndex);
                        break;
                    case ChessConstants.Pawn when ToIndex % 8 != FromIndex % 8:
                        // ep
                        Move = MoveUtil.CreateEpMove(FromIndex, ToIndex);
                        break;
                    default:
                        Move = MoveUtil.CreateMove(FromIndex, ToIndex, PieceIndex);
                        break;
                }
            }
            else
            {
                if (PieceIndex == ChessConstants.Pawn && (ToRank == 1 || ToRank == 8))
                {
                    if (moveString.Length == 5)
                    {
                        if (moveString.Substring(4, 1).Equals("n"))
                        {
                            IsKnightPromotion = true;
                            Move = MoveUtil.CreatePromotionAttack(MoveUtil.TypePromotionN, FromIndex, ToIndex,
                                PieceIndexAttacked);
                        }
                        else if (moveString.Substring(4, 1).Equals("r"))
                        {
                            IsRookPromotion = true;
                            Move = MoveUtil.CreatePromotionAttack(MoveUtil.TypePromotionR, FromIndex, ToIndex,
                                PieceIndexAttacked);
                        }
                        else if (moveString.Substring(4, 1).Equals("b"))
                        {
                            IsBishopPromotion = true;
                            Move = MoveUtil.CreatePromotionAttack(MoveUtil.TypePromotionB, FromIndex, ToIndex,
                                PieceIndexAttacked);
                        }
                        else if (moveString.Substring(4, 1).Equals("q"))
                        {
                            IsQueenPromotion = true;
                            Move = MoveUtil.CreatePromotionAttack(MoveUtil.TypePromotionQ, FromIndex, ToIndex,
                                PieceIndexAttacked);
                        }
                    }
                    else
                    {
                        Move = MoveUtil.CreatePromotionAttack(MoveUtil.TypePromotionQ, FromIndex, ToIndex,
                            PieceIndexAttacked);
                    }
                }
                else
                {
                    Move = MoveUtil.CreateAttackMove(FromIndex, ToIndex, PieceIndex, PieceIndexAttacked);
                }
            }
        }

        public override string ToString()
        {
            var moveString = "" + FromFile + FromRank + ToFile + ToRank;
            if (IsQueenPromotion)
            {
                return moveString + "q";
            }

            if (IsKnightPromotion)
            {
                return moveString + "n";
            }

            if (IsRookPromotion)
            {
                return moveString + "r";
            }

            if (IsBishopPromotion)
            {
                return moveString + "b";
            }

            return moveString;
        }

        public override bool Equals(object obj)
        {
            var compare = (MoveWrapper) obj;
            return compare != null && compare.ToString().Equals(ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}