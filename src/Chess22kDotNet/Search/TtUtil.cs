using System;
using Chess22kDotNet.Engine;
using Chess22kDotNet.Move;

namespace Chess22kDotNet.Search
{
    public static class TtUtil
    {
        public const int FlagExact = 0;
        public const int FlagUpper = 1;
        public const int FlagLower = 2;

        private const int BucketSize = 4;
        private static int _keyShifts;

        private static TtEntry[] _entries;

        public static long HalfMoveCounter = 0;

        private static bool _isInitialized;

        public static void Init(bool force)
        {
            if (!force && _isInitialized) return;
            _keyShifts = 64 - EngineConstants.Power2TtEntries;
            var maxEntries = (int) (Util.PowerLookup[EngineConstants.Power2TtEntries] + BucketSize - 1);

            _entries = new TtEntry[maxEntries];

            _isInitialized = true;
        }

        public static void ClearValues()
        {
            for (var i = 0; i < _entries.Length; i++) _entries[i].Key = 0;
        }

        public static TtEntry GetEntry(long key)
        {
            var index = GetIndex(key);

            for (var i = index; i < index + BucketSize; i++)
            {
                if (_entries[i].Key != key) continue;
                if (Statistics.Enabled) Statistics.TtHits++;

                return _entries[i];
            }

            if (Statistics.Enabled) Statistics.TtMisses++;

            return new TtEntry();
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
            for (var i = index; i < index + BucketSize; i++)
            {
                if (_entries[i].Key == 0)
                {
                    replaceIndex = i;
                    break;
                }

                var currentEntry = _entries[i];

                var currentDepth = currentEntry.Depth;
                if (_entries[i].Key == key)
                {
                    if (currentDepth > depth && flag != FlagExact) return;

                    replaceIndex = i;
                    break;
                }

                // replace the lowest depth
                if (currentDepth >= replacedDepth) continue;
                replaceIndex = i;
                replacedDepth = currentDepth;
            }

            _entries[replaceIndex] = new TtEntry
            {
                Key = key,
                Move = move,
                Flag = (byte) flag,
                Depth = (short) depth
            };
            _entries[replaceIndex].SetScore(score, ply);
        }

        public static string ToString(TtEntry ttEntry)
        {
            return "score=" + ttEntry.GetScore(0) + " " + new MoveWrapper(ttEntry.Move) + " depth=" +
                   ttEntry.Depth + " flag="
                   + ttEntry.Flag;
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
            for (var i = 0; i < 500; i++)
                if (_entries[i].Key != 0)
                    usage++;

            return usage;
        }

        public static bool CanRefineEval(TtEntry ttEntry, int eval, int score)
        {
            if (ttEntry.Key == 0) return false;
            return ttEntry.Flag == FlagExact || ttEntry.Flag == FlagUpper && score < eval ||
                   ttEntry.Flag == FlagLower && score > eval;
        }
    }
}