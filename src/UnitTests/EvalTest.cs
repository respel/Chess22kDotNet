using Chess22kDotNet.Eval;
using Chess22kDotNet.Search;

namespace Chess22kDotNet.UnitTests
{
    public class EvalTest
    {
        public static void Main()
        {
            var cb = ChessBoardInstances.Get(0);
            ChessBoardUtil.SetFen("1r1q1rk1/2p1npb1/b3p1p1/p5N1/1ppPB2R/P1N1P1P1/1P2QPP1/2K4R w - - 0 20 ", cb);
            EvalUtil.CalculateScore(cb, ThreadData.GetInstance(0));
        }
    }
}