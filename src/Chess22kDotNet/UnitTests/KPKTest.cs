using System;
using System.Numerics;
using Chess22kDotNet.Eval;
using Chess22kDotNet.Texel;

namespace Chess22kDotNet.UnitTests
{
    public class KpkTest
    {
        public static void Main()
        {
            // read all fens, including score
            var fens = Tuner.LoadFens("d:\\backup\\chess\\epds\\quiet-labeled.epd", true, true);
            Console.WriteLine("Fens found : " + fens.Count);

            var tested = 0;
            var ok = 0;
            var nok = 0;
            foreach (var (fen, score) in fens)
            {
                var cb = ChessBoardInstances.Get(0);
                ChessBoardUtil.SetFen(fen, cb);
                if (BitOperations.PopCount((ulong) cb.AllPieces) > 3)
                {
                    continue;
                }

                if (cb.Pieces[ChessConstants.White][ChessConstants.Pawn] == 0 &&
                    cb.Pieces[ChessConstants.Black][ChessConstants.Pawn] == 0)
                {
                    continue;
                }

                tested++;
                if (KpkBitbase.IsDraw(cb) == (score == 0.5))
                {
                    ok++;
                }
                else
                {
                    nok++;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Tested " + tested);
            Console.WriteLine("OK " + ok);
            Console.WriteLine("NOK " + nok);
        }
    }
}