using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chess22kDotNet.Move;

namespace Chess22kDotNet.MainTests
{
    public class Epd
    {
        private readonly string _fen;
        private readonly string _id;

        private readonly List<string> _moveStrings = new List<string>();

        // 3r1k2/4npp1/1ppr3p/p6P/P2PPPP1/1NR5/5K2/2R5 w - - bm d5; id \"BK.02\";
        // r1bqk1nr/pppnbppp/3p4/8/2BNP3/8/PPP2PPP/RNBQK2R w KQkq - bm Bxf7+; id \"CCR12\";
        // 6k1/p3q2p/1nr3pB/8/3Q1P2/6P1/PP5P/3R2K1 b - - bm Rd6; id \"position12\";
        public Epd(string epdString)
        {
            var tokens = epdString.Split(";");

            // fen
            var fenToken = tokens[0].Split(" ");
            _fen = fenToken[0] + " " + fenToken[1] + " " + fenToken[2] + " " + fenToken[3];
            IsBestMove = fenToken[4].Equals("bm");

            // there could be multiple best-moves
            for (var i = 5; i < fenToken.Length; i++)
            {
                // remove check indication
                var moveString = fenToken[i];
                if (moveString.EndsWith("+")) moveString = moveString.Replace("+", "");

                // remove capture indication
                if (moveString.Contains("x")) moveString = moveString.Replace("x", "");

                _moveStrings.Add(moveString);
            }

            // id
            var idToken = tokens[1];
            _id = idToken.Split(" ")[2].Replace("\"", "");
        }

        /**
         * bm or am
         */
        public bool IsBestMove { get; }

        public bool MoveEquals(MoveWrapper bestMove)
        {
            return _moveStrings.Any(moveString => MoveEquals(moveString, bestMove));
        }

        private static bool MoveEquals(string moveString, MoveWrapper bestMove)
        {
            var move = bestMove.Move;
            var sourceIndex = MoveUtil.GetSourcePieceIndex(move);
            return moveString.Length switch
            {
                2 => sourceIndex == ChessConstants.Pawn && moveString.Substring(0, 1).Equals(bestMove.ToFile + "") &&
                     moveString.Substring(1, 1).Equals(bestMove.ToRank + ""),
                3 when moveString.Substring(0, 1).ToLower().Equals(moveString.Substring(0, 1)) =>
                    sourceIndex == ChessConstants.Pawn && moveString.Substring(0, 1).Equals(bestMove.FromFile + "") &&
                    moveString.Substring(1, 1).Equals(bestMove.ToFile + "") &&
                    moveString.Substring(2, 1).Equals(bestMove.ToRank + ""),
                3 => moveString.Substring(0, 1).Equals(ChessConstants.FenWhitePieces[sourceIndex]) &&
                     moveString.Substring(1, 1).Equals(bestMove.ToFile + "") &&
                     moveString.Substring(2, 1).Equals(bestMove.ToRank + ""),
                4 => moveString.Substring(0, 1).Equals(ChessConstants.FenWhitePieces[sourceIndex]) &&
                     moveString.Substring(1, 1).Equals(bestMove.FromFile + "") &&
                     moveString.Substring(2, 1).Equals(bestMove.ToFile + "") &&
                     moveString.Substring(3, 1).Equals(bestMove.ToRank + ""),
                _ => throw new ArgumentException("Unknown move string: " + moveString)
            };
        }

        public string GetId()
        {
            return _id;
        }

        public string GetFen()
        {
            return _fen;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var moveString in _moveStrings) sb.Append(moveString).Append(" ");

            return sb.ToString();
        }
    }
}