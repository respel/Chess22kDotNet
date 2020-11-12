using Chess22kDotNet.Search;

namespace Chess22kDotNet.Engine
{
    public static class UciOptions
    {
        public static int ThreadCount = 1;
        public static bool Ponder = true;

        public static void SetThreadCount(int threadCount)
        {
            if (threadCount == ThreadCount) return;
            ThreadCount = threadCount;
            ChessBoardInstances.Init(threadCount);
            ThreadData.InitInstances(threadCount);
        }

        public static void SetPonder(bool ponder)
        {
            Ponder = ponder;
        }
    }
}