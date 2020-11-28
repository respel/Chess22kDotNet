using Chess22kDotNet.Engine;

namespace Chess22kDotNet.Eval
{
    public static class PawnCacheUtil
    {
        private const int Power2TableShifts = 64 - EngineConstants.Power2PawnEvalEntries;

        public static int UpdateBoardAndGetScore(ChessBoard cb, long[] pawnCache)
        {
            if (!EngineConstants.EnablePawnEvalCache) return ChessConstants.CacheMiss;

            var index = GetIndex(cb.PawnZobristKey);
            if (pawnCache[index] == cb.PawnZobristKey)
            {
                if (Statistics.Enabled) Statistics.PawnEvalCacheHits++;

                if (!EngineConstants.TestEvalCaches) cb.PassedPawnsAndOutposts = pawnCache[index + 1];

                return (int) pawnCache[index + 2];
            }

            if (Statistics.Enabled) Statistics.PawnEvalCacheMisses++;

            return ChessConstants.CacheMiss;
        }

        public static void AddValue(long key, int score, long passedPawnsAndOutpostsValue, long[] pawnCache)
        {
            if (!EngineConstants.EnablePawnEvalCache) return;

            if (EngineConstants.Assert)
            {
                Assert.IsTrue(score <= Util.ShortMax);
                Assert.IsTrue(score >= Util.ShortMin);
            }

            var index = GetIndex(key);
            pawnCache[index] = key;
            pawnCache[index + 1] = passedPawnsAndOutpostsValue;
            pawnCache[index + 2] = score;
        }

        private static int GetIndex(long key)
        {
            return (int) Util.RightTripleShift(key, Power2TableShifts) * 3;
        }
    }
}