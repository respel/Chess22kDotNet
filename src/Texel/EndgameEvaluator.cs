using System;
using Chess22kDotNet.Eval;
using Chess22kDotNet.Search;

namespace Chess22kDotNet.Texel
{
    public class EndgameEvaluator
    {
        public static void Main()
        {
            var fens = Tuner.LoadFens("d:\\backup\\chess\\epds\\quiet-labeled.epd", true, false);
            Console.WriteLine(fens.Count + " fens found");

            var kpk = new ErrorCount("KPK ");
            var kbnk = new ErrorCount("KBNK");
            var kbpk = new ErrorCount("KBPK");
            var krkp = new ErrorCount("KRKP");
            var kqkp = new ErrorCount("KQKP");
            var krkb = new ErrorCount("KRKB");
            var krkn = new ErrorCount("KRKN");
            var kbpkp = new ErrorCount("KBPKP");
            var krbkb = new ErrorCount("KRBKB");
            var krbkr = new ErrorCount("KRBKR");

            var cb = ChessBoardInstances.Get(0);
            var threadData = ThreadData.GetInstance(0);
            foreach (var (key, value) in fens)
            {
                ChessBoardUtil.SetFen(key, cb);

                var error = Math.Pow(
                    value - ErrorCalculator.CalculateSigmoid(ChessConstants.ColorFactor[cb.ColorToMove] *
                                                             EvalUtil.CalculateScore(cb, threadData)),
                    2);
                if (MaterialUtil.IsKbnk(cb.MaterialKey))
                {
                    kbnk.AddError(error);
                }
                else if (MaterialUtil.IsKqkp(cb.MaterialKey))
                {
                    kqkp.AddError(error);
                }
                else if (MaterialUtil.IsKrkp(cb.MaterialKey))
                {
                    krkp.AddError(error);
                }
                else if (MaterialUtil.IsKrkb(cb.MaterialKey))
                {
                    krkb.AddError(error);
                }
                else if (MaterialUtil.IsKrkn(cb.MaterialKey))
                {
                    krkn.AddError(error);
                }
                else if (MaterialUtil.IsKpk(cb.MaterialKey))
                {
                    krkn.AddError(error);
                }
                else if (MaterialUtil.IsKbpk(cb.MaterialKey))
                {
                    kbpk.AddError(error);
                }
                else if (MaterialUtil.IsKbpkp(cb.MaterialKey))
                {
                    kbpkp.AddError(error);
                }
                else if (MaterialUtil.IsKrbkb(cb.MaterialKey))
                {
                    krbkb.AddError(error);
                }
                else if (MaterialUtil.IsKrbkr(cb.MaterialKey))
                {
                    krbkr.AddError(error);
                }
            }

            kpk.Print();
            kbnk.Print();
            krkp.Print();
            kqkp.Print();
            krkb.Print();
            krkn.Print();
            krbkb.Print();
            krbkr.Print();
            kbpk.Print();
            kbpkp.Print();
        }

        private class ErrorCount
        {
            private int _count;
            private double _totalError;
            private readonly string _name;

            public ErrorCount(string name)
            {
                _name = name;
            }

            public void AddError(double error)
            {
                _totalError += error;
                _count++;
            }

            public void Print()
            {
                if (_count == 0)
                {
                    Console.WriteLine(_name + " 0");
                }
                else
                {
                    Console.WriteLine($"{_name} {_totalError / _count}");
                }
            }
        }
    }
}