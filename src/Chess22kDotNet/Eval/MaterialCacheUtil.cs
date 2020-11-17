using Chess22kDotNet.Engine;

namespace Chess22kDotNet.Eval
{
    public static class MaterialCacheUtil
    {
        private const int Power2TableShifts = 64 - EngineConstants.Power2MaterialEntries;

        public static int GetScore(int key, int[] materialCache)
        {
            if (!EngineConstants.EnableMaterialCache)
            {
                return ChessConstants.CacheMiss;
            }

            var index = GetIndex(key);

            if (materialCache[index] == key)
            {
                if (Statistics.Enabled)
                {
                    Statistics.MaterialCacheHits++;
                }

                return materialCache[index + 1];
            }

            if (Statistics.Enabled)
            {
                Statistics.MaterialCacheMisses++;
            }

            return ChessConstants.CacheMiss;
        }

        public static void AddValue(int key, int score, int[] materialCache)
        {
            if (!EngineConstants.EnableMaterialCache)
            {
                return;
            }

            if (EngineConstants.Assert)
            {
                Assert.IsTrue(score <= Util.ShortMax);
                Assert.IsTrue(score >= Util.ShortMin);
            }

            var index = GetIndex(key);
            materialCache[index] = key;
            materialCache[index + 1] = score;
        }

        private static int GetIndex(int materialKey)
        {
            return Util.RightTripleShift(materialKey * 836519301, Power2TableShifts) << 1;
        }
    }
}