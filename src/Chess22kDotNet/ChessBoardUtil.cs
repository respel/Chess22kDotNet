using System;
using System.Numerics;
using System.Text;
using Chess22kDotNet.Engine;
using Chess22kDotNet.Eval;
using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet
{
    public static class ChessBoardUtil
    {
        public static void SetStartFen(ChessBoard cb)
        {
            SetFen(FenStart, cb);
        }

        public static void SetFen(string fen, ChessBoard cb)
        {
            cb.MoveCounter = 0;

            var fenArray = fen.Split(" ");

            // 1: pieces: rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR
            SetPieces(cb, fenArray[0]);

            // 2: active-color: w
            cb.ColorToMove = fenArray[1] == "w" ? White : Black;

            // 3: castling: KQkq
            cb.CastlingRights = 15;
            if (fenArray.Length > 2)
            {
                if (!fenArray[2].Contains("K")) cb.CastlingRights &= 7;

                if (!fenArray[2].Contains("Q")) cb.CastlingRights &= 11;

                if (!fenArray[2].Contains("k")) cb.CastlingRights &= 13;

                if (!fenArray[2].Contains("q")) cb.CastlingRights &= 14;
            }
            else
            {
                // try to guess the castling rights
                if (cb.KingIndex[White] != 3) cb.CastlingRights &= 3; // 0011

                if (cb.KingIndex[Black] != 59) cb.CastlingRights &= 12; // 1100
            }

            if (fenArray.Length > 3)
            {
                // 4: en-passant: -
                if (fenArray[3] == "-" || fenArray[3] == "–")
                    cb.EpIndex = 0;
                else
                    cb.EpIndex = 104 - fenArray[3][0] + 8 * (int.Parse(fenArray[3].Substring(1)) - 1);
            }

            if (fenArray.Length > 4)
            {
                // TODO
                // 5: half-counter since last capture or pawn advance: 1
                // fenArray[4]

                // 6: counter: 1
                cb.MoveCounter = int.Parse(fenArray[5]) * 2;
                if (cb.ColorToMove == Black) cb.MoveCounter++;
            }
            else
            {
                // if counter is not set, try to guess
                // assume in the beginning every 2 moves, a pawn is moved
                var pawnsNotAtStartingPosition =
                    16 - BitOperations.PopCount((ulong)(cb.Pieces[White][Pawn] & Bitboard.Rank2))
                       - BitOperations.PopCount((ulong)(cb.Pieces[Black][Pawn] & Bitboard.Rank7));
                cb.MoveCounter = pawnsNotAtStartingPosition * 2;
            }

            Init(cb);
        }

        // rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR
        private static void SetPieces(ChessBoard cb, string fenPieces)
        {
            // clear pieces
            for (var color = 0; color < 2; color++)
                for (var pieceIndex = 1; pieceIndex <= King; pieceIndex++)
                    cb.Pieces[color][pieceIndex] = 0;

            var positionCount = 63;
            foreach (var character in fenPieces)
                switch (character)
                {
                    case '/':
                        continue;
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                        positionCount -= character - '0';
                        break;
                    case 'P':
                        cb.Pieces[White][Pawn] |= Util.PowerLookup[positionCount--];
                        break;
                    case 'N':
                        cb.Pieces[White][Knight] |= Util.PowerLookup[positionCount--];
                        break;
                    case 'B':
                        cb.Pieces[White][Bishop] |= Util.PowerLookup[positionCount--];
                        break;
                    case 'R':
                        cb.Pieces[White][Rook] |= Util.PowerLookup[positionCount--];
                        break;
                    case 'Q':
                        cb.Pieces[White][Queen] |= Util.PowerLookup[positionCount--];
                        break;
                    case 'K':
                        cb.Pieces[White][King] |= Util.PowerLookup[positionCount--];
                        break;
                    case 'p':
                        cb.Pieces[Black][Pawn] |= Util.PowerLookup[positionCount--];
                        break;
                    case 'n':
                        cb.Pieces[Black][Knight] |= Util.PowerLookup[positionCount--];
                        break;
                    case 'b':
                        cb.Pieces[Black][Bishop] |= Util.PowerLookup[positionCount--];
                        break;
                    case 'r':
                        cb.Pieces[Black][Rook] |= Util.PowerLookup[positionCount--];
                        break;
                    case 'q':
                        cb.Pieces[Black][Queen] |= Util.PowerLookup[positionCount--];
                        break;
                    case 'k':
                        cb.Pieces[Black][King] |= Util.PowerLookup[positionCount--];
                        break;
                }
        }

        public static void Copy(ChessBoard source, ChessBoard target)
        {
            // primitives
            target.CastlingRights = source.CastlingRights;
            target.PsqtScore = source.PsqtScore;
            target.ColorToMove = source.ColorToMove;
            target.ColorToMoveInverse = source.ColorToMoveInverse;
            target.EpIndex = source.EpIndex;
            target.MaterialKey = source.MaterialKey;
            target.Phase = source.Phase;
            target.AllPieces = source.AllPieces;
            target.EmptySpaces = source.EmptySpaces;
            target.ZobristKey = source.ZobristKey;
            target.PawnZobristKey = source.PawnZobristKey;
            target.CheckingPieces = source.CheckingPieces;
            target.PinnedPieces = source.PinnedPieces;
            target.DiscoveredPieces = source.DiscoveredPieces;
            target.MoveCounter = source.MoveCounter;
            target.MoveCount = source.MoveCount;

            // small arrays
            target.KingIndex[White] = source.KingIndex[White];
            target.KingIndex[Black] = source.KingIndex[Black];

            // large arrays
            Array.Copy(source.PieceIndexes, 0, target.PieceIndexes, 0, source.PieceIndexes.Length);
            Array.Copy(source.ZobristKeyHistory, 0, target.ZobristKeyHistory, 0, source.ZobristKeyHistory.Length);

            // multi-dimensional arrays
            Array.Copy(source.Pieces[White], 0, target.Pieces[White], 0, source.Pieces[White].Length);
            Array.Copy(source.Pieces[Black], 0, target.Pieces[Black], 0, source.Pieces[Black].Length);
        }

        public static void Init(ChessBoard cb)
        {
            MaterialUtil.SetKey(cb);

            cb.KingIndex[White] = BitOperations.TrailingZeroCount(cb.Pieces[White][King]);
            cb.KingIndex[Black] = BitOperations.TrailingZeroCount(cb.Pieces[Black][King]);

            cb.ColorToMoveInverse = 1 - cb.ColorToMove;
            cb.Pieces[White][All] = cb.Pieces[White][Pawn] | cb.Pieces[White][Bishop] | cb.Pieces[White][Knight] |
                                    cb.Pieces[White][King] | cb.Pieces[White][Rook]
                                    | cb.Pieces[White][Queen];
            cb.Pieces[Black][All] = cb.Pieces[Black][Pawn] | cb.Pieces[Black][Bishop] | cb.Pieces[Black][Knight] |
                                    cb.Pieces[Black][King] | cb.Pieces[Black][Rook]
                                    | cb.Pieces[Black][Queen];
            cb.AllPieces = cb.Pieces[White][All] | cb.Pieces[Black][All];
            cb.EmptySpaces = ~cb.AllPieces;

            Array.Fill(cb.PieceIndexes, Empty);
            foreach (var t in cb.Pieces)
                for (var pieceIndex = 1; pieceIndex < cb.Pieces[0].Length; pieceIndex++)
                {
                    var piece = t[pieceIndex];
                    while (piece != 0)
                    {
                        cb.PieceIndexes[BitOperations.TrailingZeroCount(piece)] = pieceIndex;
                        piece &= piece - 1;
                    }
                }

            cb.SetCheckingPinnedAndDiscoPieces();
            cb.PsqtScore = EvalUtil.CalculatePositionScores(cb);

            cb.Phase = EvalUtil.PhaseTotal -
                       (BitOperations.PopCount((ulong)(cb.Pieces[White][Knight] | cb.Pieces[Black][Knight])) *
                        EvalConstants.Phase[Knight]
                        + BitOperations.PopCount((ulong)(cb.Pieces[White][Bishop] | cb.Pieces[Black][Bishop])) *
                        EvalConstants.Phase[Bishop]
                        + BitOperations.PopCount((ulong)(cb.Pieces[White][Rook] | cb.Pieces[Black][Rook])) *
                        EvalConstants.Phase[Rook]
                        + BitOperations.PopCount((ulong)(cb.Pieces[White][Queen] | cb.Pieces[Black][Queen])) *
                        EvalConstants.Phase[Queen]);

            Zobrist.SetPawnKey(cb);
            Zobrist.SetKey(cb);
        }

        public static long CalculateTotalMoveCount()
        {
            long totalMoveCount = 0;
            for (var i = 0; i < UciOptions.ThreadCount; i++) totalMoveCount += ChessBoardInstances.Get(i).MoveCount;

            return totalMoveCount;
        }

        public static string ToString(ChessBoard cb)
        {
            // TODO castling, EP, moves
            var sb = new StringBuilder();
            for (var i = 63; i >= 0; i--)
            {
                sb.Append((cb.Pieces[White][All] & Util.PowerLookup[i]) != 0
                    ? FenWhitePieces[cb.PieceIndexes[i]]
                    : FenBlackPieces[cb.PieceIndexes[i]]);
                if (i % 8 == 0 && i != 0) sb.Append("/");
            }

            // color to move
            var colorToMove = cb.ColorToMove == White ? "w" : "b";
            sb.Append(" ").Append(colorToMove).Append(" ");

            // castling rights
            if (cb.CastlingRights == 0)
            {
                sb.Append("-");
            }
            else
            {
                if ((cb.CastlingRights & 8) != 0)
                    // 1000
                    sb.Append("K");

                if ((cb.CastlingRights & 4) != 0)
                    // 0100
                    sb.Append("Q");

                if ((cb.CastlingRights & 2) != 0)
                    // 0010
                    sb.Append("k");

                if ((cb.CastlingRights & 1) != 0)
                    // 0001
                    sb.Append("q");
            }

            // en passant
            sb.Append(" ");
            if (cb.EpIndex == 0)
                sb.Append("-");
            else
                sb.Append("" + (char)(104 - cb.EpIndex % 8) + (cb.EpIndex / 8 + 1));

            var fen = sb.ToString();
            fen = fen.Replace("11111111", "8");
            fen = fen.Replace("1111111", "7");
            fen = fen.Replace("111111", "6");
            fen = fen.Replace("11111", "5");
            fen = fen.Replace("1111", "4");
            fen = fen.Replace("111", "3");
            fen = fen.Replace("11", "2");

            return fen;
        }
    }
}