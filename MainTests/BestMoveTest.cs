using System;
using System.Collections.Generic;
using System.IO;
using Chess22kDotNet.Engine;
using Chess22kDotNet.Move;
using Chess22kDotNet.Search;

namespace Chess22kDotNet.MainTests
{
    public static class BestMoveTest
    {
        private static int _positionTestOk;
        private static int _positionTestNok;
        private static readonly ThreadData ThreadData = ThreadData.GetInstance(0);

        public static void Main()
        {
            UciOut.NoOutput = true;
            TtUtil.Init(false);

            DoTest(GetEpdStrings("Resources/WAC-201.epd"));
            // DoTest(GetEpdStrings("Resources/EigenmannEndgame.epd"));

            Console.WriteLine("");
            Console.WriteLine("Total: " + _positionTestOk + "/" + (_positionTestOk + _positionTestNok));
        }

        public static string[] GetEpdStrings(string fileName)
        {
            Console.WriteLine(fileName);
            Console.WriteLine();
            return File.ReadAllLines(fileName);
        }

        private static void DoTest(IReadOnlyCollection<string> epdStrings)
        {
            var correctCounter = 0;
            foreach (var epdString in epdStrings)
            {
                var epd = new Epd(epdString);
                var cb = ChessBoardInstances.Get(0);
                ChessBoardUtil.SetFen(epd.GetFen(), cb);

                TimeUtil.Reset();
                TimeUtil.SetSimpleTimeWindow(5000);
                SearchUtil.Start(cb);

                var bestMove = new MoveWrapper(ThreadData.GetBestMove());
                if (epd.IsBestMove)
                {
                    if (epd.MoveEquals(bestMove))
                    {
                        Console.WriteLine(epd.GetId() + " BM OK");
                        correctCounter++;
                        _positionTestOk++;
                    }
                    else
                    {
                        Console.WriteLine(epd.GetId() + " BM NOK " + bestMove + " - " + epd);
                        _positionTestNok++;
                    }
                }
                else
                {
                    if (epd.MoveEquals(bestMove))
                    {
                        Console.WriteLine(epd.GetId() + " AM NOK " + epd);
                        _positionTestNok++;
                    }
                    else
                    {
                        Console.WriteLine(epd.GetId() + " AM OK");
                        correctCounter++;
                        _positionTestOk++;
                    }
                }
            }

            Console.WriteLine(correctCounter + "/" + epdStrings.Count);
        }
    }
}