namespace Chess22kDotNet.Search
{
    public struct TtEntry
    {
        private short _depth;
        public long Key { get; set; }
        public int Move { get; set; }
        public short Score { get; set; }
        public byte Flag { get; set; }

        public short Depth
        {
            set => _depth = (short)(value + TtUtil.HalfMoveCounter);
            get => (short)(_depth - TtUtil.HalfMoveCounter);
        }
    }
}