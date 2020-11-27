using Chess22kDotNet.Engine;
using Chess22kDotNet.Eval;

namespace Chess22kDotNet.Search
{
    public struct TtEntry
    {
        private short _depth;
        private short _score;
        public long Key { get; set; }
        public int Move { get; set; }
        public byte Flag { get; set; }

        public short Depth
        {
            set
            {
                if (EngineConstants.Assert)
                {
                    Assert.IsTrue(value <= 255);
                }
                _depth = (short)(value + TtUtil.HalfMoveCounter);
            }
            get => (short)(_depth - TtUtil.HalfMoveCounter);
        }
        
        public int GetScore(int ply)
        {
            var score = (int) _score;

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

        public void SetScore(int score, int ply)
        {
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

            _score = (short) score;
        }
    }
}