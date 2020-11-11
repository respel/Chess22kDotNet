using Chess22kDotNet.Engine;

namespace Chess22kDotNet
{
    public static class ChessBoardInstances
    {
        private static ChessBoard[] _instances;

        static ChessBoardInstances()
        {
            Init(EngineConstants.TestEvalValues ? 2 : UciOptions.ThreadCount);
        }

        public static ChessBoard Get(in int instanceNumber)
        {
            return _instances[instanceNumber];
        }

        public static void Init(in int numberOfInstances)
        {
            _instances = new ChessBoard[numberOfInstances];
            for (var i = 0; i < numberOfInstances; i++)
            {
                _instances[i] = new ChessBoard();
            }
        }
    }
}