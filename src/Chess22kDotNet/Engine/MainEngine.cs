using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chess22kDotNet.JavaWrappers;
using Chess22kDotNet.Move;
using Chess22kDotNet.Search;

namespace Chess22kDotNet.Engine
{
    public static class MainEngine
    {
        private static ChessBoard _cb;
        private static ThreadData _threadData;

        public static bool Pondering;
        private static bool _maxTimeExceeded;

        public static int MaxDepth = EngineConstants.MaxPlies;

        private static void SearchTask()
        {
            try
            {
                var source = new CancellationTokenSource();
                Task.Run(async () => await MaxTimeTask(source.Token), source.Token);
                Task.Run(async () => await InfoTask(source.Token), source.Token);
                _maxTimeExceeded = false;
                SearchUtil.Start(_cb);

                // calculation ready
                source.Cancel();

                UciOut.SendBestMove(_threadData);
            }
            catch (Exception e)
            {
                ErrorLogger.Log(_cb, e, true);
            }
        }

        private static async Task MaxTimeTask(CancellationToken cancellationToken)
        {
            await Task.Delay((int) TimeUtil.GetMaxTimeMs(), cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            if (Pondering)
            {
                _maxTimeExceeded = true;
            }
            else if (_threadData.GetBestMove() != 0)
            {
                Console.WriteLine("info string max time exceeded");
                NegamaxUtil.IsRunning = false;
            }
        }
        
        private static async Task InfoTask(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(2000, cancellationToken);
                UciOut.SendInfo();
            }
        }

        public static void Main()
        {
            Thread.CurrentThread.Name = "Chess22kDotNet-main";
            _cb = ChessBoardInstances.Get(0);
            _threadData = ThreadData.GetInstance(0);
            
            Start();
        }

        private static void Start()
        {
            try
            {
                string line;
                while ((line = Console.ReadLine()) != null)
                {
                    ReadLine(line);
                }
            }
            catch (Exception e)
            {
                ErrorLogger.Log(_cb, e, true);
            }
        }

        private static void ReadLine(string line)
        {
            var tokens = line.Split(" ");
            if (tokens[0].Equals("uci"))
            {
                UciOut.SendUci();
            }
            else if (tokens[0].Equals("isready"))
            {
                Console.WriteLine("readyok");
            }
            else if (tokens[0].Equals("ucinewgame"))
            {
                TtUtil.Init(false);
                TtUtil.ClearValues();
            }
            else if (tokens[0].Equals("position"))
            {
                Position(tokens);
            }
            else if (tokens[0].Equals("go"))
            {
                Go(tokens);
            }
            else if (tokens[0].Equals("ponderhit"))
            {
                Pondering = false;
                if (_maxTimeExceeded)
                {
                    NegamaxUtil.IsRunning = false;
                }
            }
            else if (tokens[0].Equals("eval"))
            {
                UciOut.Eval(_cb, _threadData);
            }
            else if (tokens[0].Equals("setoption"))
            {
                if (tokens.Length > 4)
                {
                    SetOption(tokens[2], tokens[4]);
                }
            }
            else if (tokens[0].Equals("quit"))
            {
                Environment.Exit(0);
            }
            else if (tokens[0].Equals("stop"))
            {
                NegamaxUtil.IsRunning = false;
            }
            else
            {
                Console.WriteLine("Unknown command: " + tokens[0]);
            }
        }

        private static void Position(string[] tokens)
        {
            if (tokens[1].Equals("startpos"))
            {
                ChessBoardUtil.SetStartFen(_cb);
                DoMoves(tokens.Length == 2
                    // position startpos
                    ? new string[] { }
                    // position startpos moves f2f3 g1a3 ...
                    : Arrays.CopyOfRange(tokens, 3, tokens.Length));
            }
            else
            {
                // position fen 4k3/8/8/8/8/3K4 b kq - 0 1 moves f2f3 g1a3 ...
                var fen = tokens[2] + " " + tokens[3] + " " + tokens[4] + " " + tokens[5];
                if (tokens.Length > 6)
                {
                    fen += " " + tokens[6];
                    fen += " " + tokens[7];
                }

                ChessBoardUtil.SetFen(fen, _cb);

                if (tokens.Length == 6 || tokens.Length == 7 || tokens.Length == 8)
                {
                    // position fen 4k3/8/8/8/8/3K4 b kq - 0 1
                    DoMoves(new string[] { });
                }
                else
                {
                    // position fen 4k3/8/8/8/8/3K4 b kq - 0 1 moves f2f3 g1a3 ...
                    DoMoves(Arrays.CopyOfRange(tokens, 9, tokens.Length));
                }
            }

            ErrorLogger.StartFen = _cb.ToString();
            TtUtil.HalfMoveCounter = _cb.MoveCounter;
        }

        private static void SetOption(string optionName, string optionValue)
        {
            // setoption name Hash value 128
            if (optionName.ToLower().Equals("hash"))
            {
                var value = int.Parse(optionValue);
                TtUtil.SetSizeMb(value);
            }
            else if (optionName.ToLower().Equals("threads"))
            {
                UciOptions.SetThreadCount(int.Parse(optionValue));
                _cb = ChessBoardInstances.Get(0);
                _threadData = ThreadData.GetInstance(0);
            }
            else if (optionName.ToLower().Equals("ponder"))
            {
                UciOptions.SetPonder(bool.Parse(optionValue));
            }
            else
            {
                Console.WriteLine("Unknown option: " + optionName);
            }
        }

        private static void Go(IReadOnlyList<string> goCommandTokens)
        {
            // go movestogo 30 wtime 3600000 btime 3600000
            // go wtime 40847 btime 48019 winc 0 binc 0 movestogo 20

            Statistics.Reset();
            TimeUtil.Reset();
            TimeUtil.SetMoveCount(_cb.MoveCounter);
            MaxDepth = EngineConstants.MaxPlies;
            Pondering = false;

            TtUtil.Init(false);
            var ttEntry = TtUtil.GetEntry(_cb.ZobristKey);
            if (ttEntry.Key != 0 && ttEntry.Flag == TtUtil.FlagExact)
            {
                TimeUtil.SetTtHit();
            }

            // go
            // go infinite
            // go ponder
            if (goCommandTokens.Count != 1)
            {
                for (var i = 1; i < goCommandTokens.Count; i++)
                {
                    if (goCommandTokens[i].Equals("infinite"))
                    {
                        // TODO are we clearing the values again?
                        TtUtil.ClearValues();
                    }
                    else if (goCommandTokens[i].Equals("ponder"))
                    {
                        Pondering = true;
                    }
                    else if (goCommandTokens[i].Equals("movetime"))
                    {
                        var s = goCommandTokens[i + 1];
                        TimeUtil.SetExactMoveTime(int.Parse(s));
                    }
                    else if (goCommandTokens[i].Equals("movestogo"))
                    {
                        var s = goCommandTokens[i + 1];
                        TimeUtil.SetMovesToGo(int.Parse(s));
                    }
                    else if (goCommandTokens[i].Equals("depth"))
                    {
                        var s = goCommandTokens[i + 1];
                        MaxDepth = int.Parse(s);
                    }
                    else if (goCommandTokens[i].Equals("wtime"))
                    {
                        if (_cb.ColorToMove != ChessConstants.White) continue;
                        var s = goCommandTokens[i + 1];
                        TimeUtil.SetTotalTimeLeft(int.Parse(s));
                    }
                    else if (goCommandTokens[i].Equals("btime"))
                    {
                        if (_cb.ColorToMove != ChessConstants.Black) continue;
                        var s = goCommandTokens[i + 1];
                        TimeUtil.SetTotalTimeLeft(int.Parse(s));
                    }
                    else if (goCommandTokens[i].Equals("winc") || goCommandTokens[i].Equals("binc"))
                    {
                        var s = goCommandTokens[i + 1];
                        TimeUtil.SetIncrement(int.Parse(s));
                    }
                }
            }

            TimeUtil.Start();
            
            Task.Run(SearchTask);
        }

        private static void DoMoves(IEnumerable<string> moveTokens)
        {
            // apply moves
            foreach (var moveToken in moveTokens)
            {
                var move = new MoveWrapper(moveToken, _cb);
                _cb.DoMove(move.Move);
            }
        }
    }
}