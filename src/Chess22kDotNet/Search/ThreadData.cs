using System;
using System.Text;
using Chess22kDotNet.Engine;
using Chess22kDotNet.Move;
using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet.Search
{
    public class ThreadData
    {
        private static ThreadData[] _instances;
        private readonly int[][] _bfMoves = Util.CreateJaggedArray<int[][]>(2, 64 * 64);

        private readonly int[][][] _counterMoves = Util.CreateJaggedArray<int[][][]>(2, 7, 64);

        private readonly int[][] _hhMoves = Util.CreateJaggedArray<int[][]>(2, 64 * 64);
        private readonly int[] _killerMove1 = new int[EngineConstants.MaxPlies * 2];
        private readonly int[] _killerMove2 = new int[EngineConstants.MaxPlies * 2];

        private readonly int[] _moves = new int[1500];
        private readonly int[] _moveScores = new int[1500];

        private readonly int[] _nextToGenerate = new int[EngineConstants.MaxPlies * 2];
        private readonly int[] _nextToMove = new int[EngineConstants.MaxPlies * 2];

        private readonly int _threadNumber;

        // keys, scores
        public readonly int[] EvalCache = new int[(1 << EngineConstants.Power2EvalEntries) * 2];

        // keys, scores
        public readonly int[] MaterialCache = new int[(1 << EngineConstants.Power2MaterialEntries) * 2];

        // keys, passedPawnsOutposts, scores
        public readonly long[] PawnCache = new long[(1 << EngineConstants.Power2PawnEvalEntries) * 3];

        public readonly int[] Pv;
        private int _ply;
        public int BestScore;
        public int Depth;
        public ScoreType ScoreType;

        static ThreadData()
        {
            InitInstances(UciOptions.ThreadCount);
        }

        public ThreadData(int threadNumber)
        {
            ClearHistoryHeuristics();
            _threadNumber = threadNumber;
            if (threadNumber == 0) Pv = new int[EngineConstants.PvLength];
        }

        public static ThreadData GetInstance(int instanceNumber)
        {
            return _instances[instanceNumber];
        }

        public static void InitInstances(int nrOfInstances)
        {
            _instances = new ThreadData[nrOfInstances];
            for (var i = 0; i < _instances.Length; i++) _instances[i] = new ThreadData(i);
        }

        public void SetBestMove(ChessBoard cb, int bestMove, int alpha, int beta, int bestScore,
            int depth)
        {
            if (_threadNumber != 0) return;

            BestScore = bestScore;
            Depth = depth;
            if (bestScore <= alpha)
                ScoreType = ScoreType.Upper;
            else if (bestScore >= beta)
                ScoreType = ScoreType.Lower;
            else
                ScoreType = ScoreType.Exact;

            PvUtil.Set(cb, Pv, bestMove);
        }

        public void InitPv(ChessBoard cb)
        {
            var ttEntry = TtUtil.GetEntry(cb.ZobristKey);
            if (ttEntry.Key == 0 || ttEntry.Move == 0)
                Array.Fill(Pv, 0);
            else
                SetBestMove(cb, ttEntry.Move, Util.ShortMin, Util.ShortMax, ttEntry.GetScore(0),
                    ttEntry.Depth);
        }

        public int GetBestMove()
        {
            return Pv[0];
        }

        public int GetPonderMove()
        {
            return Pv[1];
        }

        public void ClearCaches()
        {
            Array.Fill(EvalCache, 0);
            Array.Fill(PawnCache, 0);
            Array.Fill(MaterialCache, 0);
        }

        public void ClearHistoryHeuristics()
        {
            Array.Fill(_hhMoves[White], 1);
            Array.Fill(_hhMoves[Black], 1);
            Array.Fill(_bfMoves[White], 1);
            Array.Fill(_bfMoves[Black], 1);
        }

        public void AddHhValue(int color, int move, int depth)
        {
            _hhMoves[color][MoveUtil.GetFromToIndex(move)] += depth * depth;
            if (EngineConstants.Assert) Assert.IsTrue(_hhMoves[color][MoveUtil.GetFromToIndex(move)] >= 0);
        }

        public void AddBfValue(int color, int move, int depth)
        {
            _bfMoves[color][MoveUtil.GetFromToIndex(move)] += depth * depth;
            if (EngineConstants.Assert) Assert.IsTrue(_bfMoves[color][MoveUtil.GetFromToIndex(move)] >= 0);
        }

        private int GetHhScore(int color, int fromToIndex)
        {
            if (!EngineConstants.EnableHistoryHeuristic) return 1;

            return 100 * _hhMoves[color][fromToIndex] / _bfMoves[color][fromToIndex];
        }

        public void AddKillerMove(int move, int ply)
        {
            if (!EngineConstants.EnableKillerMoves) return;
            if (_killerMove1[ply] == move) return;
            _killerMove2[ply] = _killerMove1[ply];
            _killerMove1[ply] = move;
        }

        public void AddCounterMove(int color, int parentMove, int counterMove)
        {
            if (EngineConstants.EnableCounterMoves)
                _counterMoves[color][MoveUtil.GetSourcePieceIndex(parentMove)][MoveUtil.GetToIndex(parentMove)] =
                    counterMove;
        }

        public int GetCounter(int color, int parentMove)
        {
            return _counterMoves[color][MoveUtil.GetSourcePieceIndex(parentMove)][MoveUtil.GetToIndex(parentMove)];
        }

        public int GetKiller1(int ply)
        {
            return _killerMove1[ply];
        }

        public int GetKiller2(int ply)
        {
            return _killerMove2[ply];
        }

        public void StartPly()
        {
            _nextToGenerate[_ply + 1] = _nextToGenerate[_ply];
            _nextToMove[_ply + 1] = _nextToGenerate[_ply];
            _ply++;
        }

        public void EndPly()
        {
            _ply--;
        }

        public int Next()
        {
            return _moves[_nextToMove[_ply]++];
        }

        public int GetMoveScore()
        {
            return _moveScores[_nextToMove[_ply] - 1];
        }

        public int Previous()
        {
            return _moves[_nextToMove[_ply] - 1];
        }

        public bool HasNext()
        {
            return _nextToGenerate[_ply] != _nextToMove[_ply];
        }

        public void AddMove(int move)
        {
            _moves[_nextToGenerate[_ply]++] = move;
        }

        public void SetMvvlvaScores()
        {
            for (var j = _nextToMove[_ply]; j < _nextToGenerate[_ply]; j++)
            {
                _moveScores[j] = MoveUtil.GetAttackedPieceIndex(_moves[j]) * 6 -
                                 MoveUtil.GetSourcePieceIndex(_moves[j]);
                if (MoveUtil.GetMoveType(_moves[j]) == MoveUtil.TypePromotionQ) _moveScores[j] += Queen * 6;
            }
        }

        public void SetHhScores(int colorToMove)
        {
            for (var j = _nextToMove[_ply]; j < _nextToGenerate[_ply]; j++)
                _moveScores[j] = GetHhScore(colorToMove, MoveUtil.GetFromToIndex(_moves[j]));
        }

        public void Sort()
        {
            var left = _nextToMove[_ply];
            for (int i = left, j = i; i < _nextToGenerate[_ply] - 1; j = ++i)
            {
                var score = _moveScores[i + 1];
                var move = _moves[i + 1];
                while (score > _moveScores[j])
                {
                    _moveScores[j + 1] = _moveScores[j];
                    _moves[j + 1] = _moves[j];
                    if (j-- == left) break;
                }

                _moveScores[j + 1] = score;
                _moves[j + 1] = move;
            }
        }

        public string GetMovesAsString()
        {
            var sb = new StringBuilder();
            for (var j = _nextToMove[_ply]; j < _nextToGenerate[_ply]; j++)
                sb.Append(new MoveWrapper(_moves[j]) + ", ");

            return sb.ToString();
        }
    }
}