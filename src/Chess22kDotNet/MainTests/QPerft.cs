using System;
using Chess22kDotNet.Engine;
using Chess22kDotNet.Move;
using Chess22kDotNet.Search;

namespace Chess22kDotNet.MainTests
{
    public class QPerft
    {
        private static readonly ThreadData ThreadData = ThreadData.GetInstance(0);
        private static readonly ChessBoard Cb = ChessBoardInstances.Get(0);

        public static void Main()
        {
            if (!EngineConstants.GenerateBrPromotions)
            {
                throw new Exception("Generation of underpromotions must be enabled");
            }

            TestPerft1();
            TestPerft2();
            TestPerft3();
            TestPerft4();
            TestPerft5();
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            TestPerft6();
            Console.WriteLine(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - now + " rofchade = 800");
            TestPerft7();
            // testPerft8();
        }

        private static long Qperft(in ChessBoard chessBoard, in int depth)
        {
            ThreadData.StartPly();
            MoveGenerator.GenerateMoves(ThreadData, chessBoard);
            MoveGenerator.GenerateAttacks(ThreadData, chessBoard);

            long counter = 0;
            if (depth == 1)
            {
                while (ThreadData.HasNext())
                {
                    if (chessBoard.IsLegal(ThreadData.Next()))
                    {
                        counter++;
                    }
                }

                ThreadData.EndPly();
                return counter;
            }

            while (ThreadData.HasNext())
            {
                var move = ThreadData.Next();
                if (!chessBoard.IsLegal(move))
                {
                    continue;
                }

                chessBoard.DoMove(move);
                counter += Qperft(chessBoard, depth - 1);
                chessBoard.UndoMove(move);
            }

            ThreadData.EndPly();
            return counter;
        }

        public static long Qdivide(in ChessBoard chessBoard, in int depth)
        {
            ThreadData.StartPly();
            MoveGenerator.GenerateMoves(ThreadData, chessBoard);
            long counter = 0;
            while (ThreadData.HasNext())
            {
                var move = ThreadData.Next();
                if (depth == 1)
                {
                    Console.WriteLine(new MoveWrapper(move) + ": " + 1);
                    counter++;
                    continue;
                }

                chessBoard.DoMove(move);
                var divideCounter = Qperft(chessBoard, depth - 1);
                counter += divideCounter;
                chessBoard.UndoMove(move);
                Console.WriteLine(new MoveWrapper(move) + ": " + divideCounter);
            }

            return counter;
        }

        private static void TestPerft1()
        {
            Console.WriteLine(1);
            ChessBoardUtil.SetStartFen(Cb);
            Assert.IsTrue(20 == Qperft(Cb, 1));
        }

        private static void TestPerft2()
        {
            Console.WriteLine(2);
            ChessBoardUtil.SetStartFen(Cb);
            Assert.IsTrue(400 == Qperft(Cb, 2));
        }

        private static void TestPerft3()
        {
            Console.WriteLine(3);
            ChessBoardUtil.SetStartFen(Cb);
            Assert.IsTrue(8902 == Qperft(Cb, 3));
        }

        private static void TestPerft4()
        {
            Console.WriteLine(4);
            ChessBoardUtil.SetStartFen(Cb);
            Assert.IsTrue(197281 == Qperft(Cb, 4));
        }

        private static void TestPerft5()
        {
            Console.WriteLine(5);
            ChessBoardUtil.SetStartFen(Cb);
            Assert.IsTrue(4865609 == Qperft(Cb, 5));
        }

        private static void TestPerft6()
        {
            Console.WriteLine(6);
            ChessBoardUtil.SetStartFen(Cb);
            Assert.IsTrue(119060324 == Qperft(Cb, 6));
        }

        private static void TestPerft7()
        {
            Console.WriteLine(7);
            ChessBoardUtil.SetStartFen(Cb);
            Assert.IsTrue(3195901860L == Qperft(Cb, 7));
        }

        public static void TestPerft8()
        {
            Console.WriteLine(8);
            ChessBoardUtil.SetStartFen(Cb);
            Assert.IsTrue(84998978956L == Qperft(Cb, 8));
        }
    }
}