namespace Chess22kDotNet.Engine
{
    public static class EngineConstants
    {
        public const int MaxPlies = 64;
        public const int MaxMoves = 768;
        public const int MaxThreads = 64;
        public const int PvLength = 12;

        public const bool GenerateBrPromotions = false;
        public const bool Assert = false;

        public const bool TestEvalValues = false;
        public const bool TestEvalCaches = false;
        public const bool TestTtValues = false;

        // Repetition-table
        public const bool EnableRepetitionTable = true;

        // Search improvements
        public const bool EnableCounterMoves = true;
        public const bool EnableKillerMoves = true;
        public const bool EnableHistoryHeuristic = true;
        public const bool EnableAspiration = true;
        public const int AspirationWindowDelta = 20;

        // Search extensions
        public const bool EnableCheckExtension = true;

        // Search reductions
        public const bool EnableNullMove = true;
        public const bool EnableLmr = true;
        public const bool EnableLmp = true;
        public const bool EnablePvs = true;
        public const bool EnableMateDistancePruning = true;
        public const bool EnableStaticNullMove = true;
        public const bool EnableRazoring = true;
        public const bool EnableFutilityPruning = true;
        public const bool EnableSeePruning = true;
        public const bool EnableQPruneBadCaptures = true;
        public const bool EnableQFutilityPruning = true;
        public const bool UseTtScoreAsEval = true;

        // Evaluation-function
        public const bool EnableEvalCache = true;
        public const int Power2EvalEntries = 12;
        public const bool EnableMaterialCache = true;
        public const int Power2MaterialEntries = 11;
        public const bool EnablePawnEvalCache = true;
        public const int Power2PawnEvalEntries = 12;

        // TT values
        public static int Power2TtEntries = 23;
    }
}