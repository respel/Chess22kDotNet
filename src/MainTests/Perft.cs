using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chess22kDotNet.Engine;
using Chess22kDotNet.Move;
using Chess22kDotNet.Search;

namespace Chess22kDotNet.MainTests
{
    public class Perft
    {
        public static async Task Main()
        {
            if (!EngineConstants.GenerateBrPromotions)
            {
                Console.WriteLine("Generation of underpromotions is disabled!");
            }

            ChessBoardInstances.Init(8);
            ThreadData.InitInstances(8);

            var threadNr = 0;
            var kiwi = new PerftWorker("Kiwi-pete", threadNr++);
            kiwi.AddPosition("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -", 4085603, 4);

            var ep = new PerftWorker("EP", threadNr++);
            ep.AddPosition("3k4/3p4/8/K1P4r/8/8/8/8 b - - 0 1", 20757544, 7);
            ep.AddPosition("8/8/4k3/8/2p5/8/B2P2K1/8 w - - 0 1", 14047573, 7);
            ep.AddPosition("8/8/1k6/2b5/2pP4/8/5K2/8 b - d3 0 1", 21190412, 7);

            var castling = new PerftWorker("Castling", threadNr++);
            castling.AddPosition("5k2/8/8/8/8/8/8/4K2R w K - 0 1", 661072, 6);
            castling.AddPosition("3k4/8/8/8/8/8/8/R3K3 w Q - 0 1", 803711, 6);
            castling.AddPosition("r3k2r/1b4bq/8/8/8/8/7B/R3K2R w KQkq - 0 1", 1274206, 4);
            castling.AddPosition("r3k2r/8/3Q4/8/8/5q2/8/R3K2R b KQkq - 0 1", 1720476, 4);

            var promotion = new PerftWorker("Promotion", threadNr++);
            promotion.AddPosition("2K2r2/4P3/8/8/8/8/8/3k4 w - - 0 1", 60651209, 7);
            promotion.AddPosition("4k3/1P6/8/8/8/8/K7/8 w - - 0 1", 3742283, 7);
            promotion.AddPosition("8/P1k5/K7/8/8/8/8/8 w - - 0 1", 1555980, 7);

            var mate = new PerftWorker("Stalemate and Checkmate", threadNr++);
            mate.AddPosition("8/8/1P2K3/8/2n5/1q6/8/5k2 b - - 0 1", 6334638, 6);
            mate.AddPosition("K1k5/8/P7/8/8/8/8/8 w - - 0 1", 15453, 7);
            mate.AddPosition("8/k1P5/8/1K6/8/8/8/8 w - - 0 1", 2518905, 8);
            mate.AddPosition("8/8/2k5/5q2/5n2/8/5K2/8 b - - 0 1", 3114998, 6);

            var perft = new PerftWorker("Perft", threadNr);
            perft.AddPosition(ChessConstants.FenStart, 4865609, 5);

            var kiwiTask = Task.Run(() => kiwi.Run());
            var epTask = Task.Run(() => ep.Run());
            var castlingTask = Task.Run(() => castling.Run());
            var promotionTask = Task.Run(() => promotion.Run());
            var mateTask = Task.Run(() => mate.Run());
            var perftTask = Task.Run(() => perft.Run());

            await kiwiTask;
            await epTask;
            await castlingTask;
            await promotionTask;
            await mateTask;
            await perftTask;

            Console.WriteLine();
            Console.WriteLine("Done");
        }

        private static int perft(in ChessBoard cb, in ThreadData threadData, in int depth)
        {
            threadData.StartPly();
            MoveGenerator.GenerateMoves(threadData, cb);
            MoveGenerator.GenerateAttacks(threadData, cb);

            if (depth == 0)
            {
                threadData.EndPly();
                return 1;
            }

            var counter = 0;
            while (threadData.HasNext())
            {
                var move = threadData.Next();
                if (!cb.IsLegal(move))
                {
                    continue;
                }

                cb.DoMove(move);
                counter += perft(cb, threadData, depth - 1);
                cb.UndoMove(move);
            }

            threadData.EndPly();
            return counter;
        }

        public static int Divide(in ChessBoard cb, in ThreadData threadData, in int depth)
        {
            threadData.StartPly();
            MoveGenerator.GenerateMoves(threadData, cb);
            MoveGenerator.GenerateAttacks(threadData, cb);
            var counter = 0;
            while (threadData.HasNext())
            {
                var move = threadData.Next();
                cb.DoMove(move);
                var divideCounter = perft(cb, threadData, depth - 1);
                counter += divideCounter;
                cb.UndoMove(move);
                Console.WriteLine(new MoveWrapper(move) + ": " + divideCounter);
            }

            return counter;
        }

        private class PerftWorker
        {
            private readonly List<PerftPosition> _positions = new List<PerftPosition>();
            private readonly string _name;
            private readonly int _threadNumber;

            public PerftWorker(string name, int threadNumber)
            {
                _name = name;
                _threadNumber = threadNumber;
            }

            public void Run()
            {
                Console.WriteLine("Start " + _name);

                var cb = ChessBoardInstances.Get(_threadNumber);
                var threadData = ThreadData.GetInstance(_threadNumber);
                foreach (var position in _positions)
                {
                    ChessBoardUtil.SetFen(position.Fen, cb);
                    Assert.IsTrue(position.MoveCount == perft(cb, threadData, position.Depth));
                }

                Console.WriteLine("Done " + _name);
            }

            public void AddPosition(string position, int moveCount, int depth)
            {
                _positions.Add(new PerftPosition(position, moveCount, depth));
            }
        }

        private class PerftPosition
        {
            public readonly string Fen;
            public readonly int MoveCount;
            public readonly int Depth;

            public PerftPosition(string fen, int moveCount, int depth)
            {
                Fen = fen;
                MoveCount = moveCount;
                Depth = depth;
            }
        }
    }
}