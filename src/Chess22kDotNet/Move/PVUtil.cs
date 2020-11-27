using System;
using System.Collections.Generic;
using System.Text;
using Chess22kDotNet.Search;

namespace Chess22kDotNet.Move
{
    public static class PvUtil
    {
        public static void Set(ChessBoard cb, int[] moves, int bestMove)
        {
            Array.Fill(moves, 0);
            moves[0] = bestMove;
            cb.DoMove(bestMove);

            for (var i = 1; i < moves.Length; i++)
            {
                var ttEntry = TtUtil.GetEntry(cb.ZobristKey);
                if (ttEntry.Key == 0)
                {
                    break;
                }

                var move = TtUtil.GetMove(ttEntry);
                if (move == 0)
                {
                    break;
                }

                moves[i] = move;
                cb.DoMove(move);
            }

            for (var i = moves.Length - 1; i >= 0; i--)
            {
                if (moves[i] == 0)
                {
                    continue;
                }

                cb.UndoMove(moves[i]);
            }
        }

        public static string AsString(IEnumerable<int> moves)
        {
            var sb = new StringBuilder();
            foreach (var move in moves)
            {
                if (move == 0)
                {
                    break;
                }

                sb.Append(new MoveWrapper(move)).Append(" ");
            }

            return sb.ToString();
        }
    }
}