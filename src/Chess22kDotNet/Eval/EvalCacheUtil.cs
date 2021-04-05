using Chess22kDotNet.Engine;

namespace Chess22kDotNet.Eval
{
    public static class EvalCacheUtil
    {
        private const int Power2TableShifts = 64 - EngineConstants.Power2EvalEntries;

        public static int GetScore(long key, int[] evalCache)
        {
            var index = GetIndex(key);

            if (evalCache[index] == (int)key)
            {
                if (Statistics.Enabled) Statistics.EvalCacheHits++;

                return evalCache[index + 1];
            }

            if (Statistics.Enabled) Statistics.EvalCacheMisses++;

            return ChessConstants.CacheMiss;
        }

        public static void AddValue(long key, int score, int[] evalCache)
        {
            if (!EngineConstants.EnableEvalCache) return;

            if (EngineConstants.Assert)
            {
                Assert.IsTrue(score <= Util.ShortMax);
                Assert.IsTrue(score >= Util.ShortMin);
            }

            var index = GetIndex(key);
            evalCache[index] = (int)key;
            evalCache[index + 1] = score;
        }

        private static int GetIndex(long key)
        {
            return (int)Util.RightTripleShift(key, Power2TableShifts) << 1;
        }
    }
}