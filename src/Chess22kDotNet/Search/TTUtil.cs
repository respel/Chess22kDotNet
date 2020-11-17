using System;
using Chess22kDotNet.Engine;
using Chess22kDotNet.Eval;
using Chess22kDotNet.Move;

namespace Chess22kDotNet.Search
{
    public static class TtUtil
    {
        private static int _keyShifts;

        // key, value
        private static long[] _keys;

        public const int FlagExact = 0;
        public const int FlagUpper = 1;
        public const int FlagLower = 2;

        public static long HalfMoveCounter = 0;

        private const int BucketSize = 4;

        // ///////////////////// DEPTH //10 bits
        private const int Flag = 10; // 2
        private const int Move = 12; // 22
        private const int Score = 48; // 16

        private static bool _isInitialized;

        public static void Init(bool force)
        {
            if (!force && _isInitialized) return;
            _keyShifts = 64 - EngineConstants.Power2TtEntries;
            var maxEntries = (int) (Util.PowerLookup[EngineConstants.Power2TtEntries] + BucketSize - 1) * 2;

            _keys = new long[maxEntries];

            _isInitialized = true;
        }

        public static void ClearValues()
        {
            Array.Fill(_keys, 0);
        }

        public static long GetValue(long key)
        {
            var index = GetIndex(key);

            for (var i = index; i < index + BucketSize * 2; i += 2)
            {
                var xorKey = _keys[i];
                var value = _keys[i + 1];
                if ((xorKey ^ value) != key) continue;
                if (Statistics.Enabled)
                {
                    Statistics.TtHits++;
                }

                return value;
            }

            if (Statistics.Enabled)
            {
                Statistics.TtMisses++;
            }

            return 0;
        }

        private static int GetIndex(long key)
        {
            return (int) Util.RightTripleShift(key, _keyShifts) << 1;
        }

        public static void AddValue(long key, int score, int ply, int depth, int flag, int move)
        {
            if (EngineConstants.Assert)
            {
                Assert.IsTrue(depth >= 1);
                Assert.IsTrue(score >= Util.ShortMin && score <= Util.ShortMax);
                Assert.IsTrue(score != ChessConstants.ScoreNotRunning);
            }

            var index = GetIndex(key);
            long replacedDepth = int.MaxValue;
            var replaceIndex = index;
            for (var i = index; i < index + BucketSize * 2; i += 2)
            {
                var xorKey = _keys[i];
                if (xorKey == 0)
                {
                    replaceIndex = i;
                    break;
                }

                var currentValue = _keys[i + 1];

                var currentDepth = GetDepth(currentValue);
                if ((xorKey ^ currentValue) == key)
                {
                    if (currentDepth > depth && flag != FlagExact)
                    {
                        return;
                    }

                    replaceIndex = i;
                    break;
                }

                // replace the lowest depth
                if (currentDepth >= replacedDepth) continue;
                replaceIndex = i;
                replacedDepth = currentDepth;
            }

            // correct mate-score
            if (score > EvalConstants.ScoreMateBound)
            {
                score += ply;
            }
            else if (score < -EvalConstants.ScoreMateBound)
            {
                score -= ply;
            }

            if (EngineConstants.Assert)
            {
                Assert.IsTrue(score >= Util.ShortMin && score <= Util.ShortMax);
            }

            var value = CreateValue(score, move, flag, depth);
            _keys[replaceIndex] = key ^ value;
            _keys[replaceIndex + 1] = value;
        }

        public static int GetScore(long value, int ply)
        {
            var score = (int) (value >> Score);

            // correct mate-score
            if (score > EvalConstants.ScoreMateBound)
            {
                score -= ply;
            }
            else if (score < -EvalConstants.ScoreMateBound)
            {
                score += ply;
            }

            if (EngineConstants.Assert)
            {
                Assert.IsTrue(score >= Util.ShortMin && score <= Util.ShortMax);
            }

            return score;
        }

        public static int GetDepth(long value)
        {
            return (int) ((value & 0x3ff) - HalfMoveCounter);
        }

        public static int GetFlag(long value)
        {
            return (int) (Util.RightTripleShift(value, Flag) & 3);
        }

        public static int GetMove(long value)
        {
            return (int) (Util.RightTripleShift(value, Move) & 0x3fffff);
        }

        // SCORE,HALF_MOVE_COUNTER,MOVE,FLAG,DEPTH
        private static long CreateValue(long score, long move, long flag, long depth)
        {
            if (!EngineConstants.Assert)
                return score << Score | move << Move | flag << Flag | (depth + HalfMoveCounter);
            Assert.IsTrue(score >= Util.ShortMin && score <= Util.ShortMax);
            Assert.IsTrue(depth <= 255);
            return score << Score | move << Move | flag << Flag | (depth + HalfMoveCounter);
        }

        public static string ToString(long ttValue)
        {
            return "score=" + GetScore(ttValue, 0) + " " + new MoveWrapper(GetMove(ttValue)) + " depth=" +
                   GetDepth(ttValue) + " flag="
                   + GetFlag(ttValue);
        }

        public static void SetSizeMb(int value)
        {
            switch (value)
            {
                case 1:
                case 2:
                case 4:
                case 8:
                case 16:
                case 32:
                case 64:
                case 128:
                case 256:
                case 512:
                case 1024:
                case 2048:
                case 4096:
                case 8192:
                case 16384:
                case 32768:
                    var power2Entries = (int) (Math.Log(value) / Math.Log(2) + 16);
                    if (EngineConstants.Power2TtEntries != power2Entries)
                    {
                        EngineConstants.Power2TtEntries = power2Entries;
                        Init(true);
                    }

                    break;
                default:
                    throw new ArgumentException("Hash-size must be between 1-16384 mb and a multiple of 2");
            }
        }

        public static long GetUsagePercentage()
        {
            var usage = 0;
            for (var i = 0; i < 1000; i++)
            {
                if (_keys[i] != 0)
                {
                    usage++;
                }
            }

            return usage;
        }

        public static bool CanRefineEval(long ttValue, int eval, int score)
        {
            if (ttValue == 0) return false;
            return GetFlag(ttValue) == FlagExact || GetFlag(ttValue) == FlagUpper && score < eval ||
                   GetFlag(ttValue) == FlagLower && score > eval;
        }
    }
}