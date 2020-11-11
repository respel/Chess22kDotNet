using Chess22kDotNet.Search;

namespace Chess22kDotNet.UnitTests
{
    public static class MainTest
    {
        public const string FenMateIn7 = "1Q6/8/8/2K5/4k3/8/8/8 b - - 4 14 "; // 32753
        public const string FenMateIn8 = "8/K7/8/1Q3k2/8/8/8/8 b - - 2 13 "; // 32751
        public const string FenMateIn9 = "8/K7/8/1Q6/5k2/8/8/8 w - - 3 14 "; // 32749
        public const string FenMateIn10W = "8/KP6/4k3/8/8/8/8/8 w - - 1 12 "; // 32748
        public const string FenMateIn10 = "8/KP1k4/8/8/8/8/8/8 b - - 0 11 "; // 32747
        public const string FenMateIn11W = "8/K2k4/1P6/8/8/8/8/8 w - - 3 11 "; // 32746
        public const string FenMateIn11 = "2k5/K7/1P6/8/8/8/8/8 b - - 2 10 "; // 32745
        public const string FenMateIn12W = "2k5/8/KP6/8/8/8/8/8 w - - 1 10 "; // 32744
        public const string FenMateIn12 = "8/2k5/KP6/8/8/8/8/8 b - - 0 9 "; // 32743
        public const string FenMateIn21 = "2k5/8/1pP1K3/1P6/8/8/8/8 w – – 0 1";

        public const string FenStandardOpening = "r2qr1k1/2p2ppp/p3bn2/2bpN1B1/8/2NQ4/PPP2PPP/3RR1K1 b - - 3 14 ";
        public const string FenStandardMiddlegame = "2b5/1p3k2/7R/4p1rP/1qpnR3/8/P4PP1/3Q2K1 w - - 0 47 ";
        public const string FenLosingCapture = "7r/5Q2/7p/7k/P5R1/B1P3P1/3PP3/n3K3 b - - 0 44 ";
        public const string FenEndgame = "8/2p2p2/3p1k2/1p1P2p1/5P1p/4K2P/p4P2/N7 b - - 1 82";
        public const string FenEndgame2 = "8/5p2/3p1k2/3P2p1/5P2/4K3/5P2/8 b - - 1 82 ";
        public const string FenStalemate = "8/8/4k3/5p2/5K2/8/8/8 w - - 0 63 ";
        public const string FenFutility = "8/1p3pk1/1q1P1bp1/4P3/n1p1P3/P5P1/1PBQ2K1/8 w - - 0 50";
        public const string FenFutility2 = "rnb1kq1r/1p1n1pp1/p3p1P1/3pP3/3p3N/2NQ4/PPP2P2/2KR1B1R w kq - 0 15 ";

        public static void Main()
        {
            var cb = ChessBoardInstances.Get(0);
            ChessBoardUtil.SetFen(FenStandardMiddlegame, cb);
            TimeUtil.SetSimpleTimeWindow(5000);
            TtUtil.Init(false);
            SearchUtil.Start(cb);
            Statistics.Print();
        }
    }
}