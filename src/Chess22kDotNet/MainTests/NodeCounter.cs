using System;
using Chess22kDotNet.Engine;
using Chess22kDotNet.Search;

namespace Chess22kDotNet.MainTests
{
    public class NodeCounter
    {
        private const int MaxPly = 11;
        private const int NumberOfPositions = 100;

        public static void Main()
        {
            var cb = ChessBoardInstances.Get(0);

            MainEngine.MaxDepth = MaxPly;
            UciOut.NoOutput = true;
            EngineConstants.Power2TtEntries = 2;
            TtUtil.Init(false);
            long totalNodesSearched = 0;

            var epdStrings = BestMoveTest.GetEpdStrings("Resources/WAC-201.epd");
            for (var index = 0; index < NumberOfPositions; index++)
            {
                Console.WriteLine(index);
                var epdString = epdStrings[index + 20];
                Statistics.Reset();
                var epd = new Epd(epdString);
                ChessBoardUtil.SetFen(epd.GetFen(), cb);
                SearchUtil.Start(cb);
                totalNodesSearched += ChessBoardUtil.CalculateTotalMoveCount();
            }

            Console.WriteLine("Total   " + totalNodesSearched);
            Console.WriteLine("Average " + totalNodesSearched / NumberOfPositions);
        }
    }
}