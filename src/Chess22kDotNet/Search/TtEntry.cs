namespace Chess22kDotNet.Search
{
    public struct TtEntry
    {
        public long Key { get; set; }
        public int Move { get; set; }
        public short Score { get; set; }
        public byte Flag { get; set; }
        public short Depth { get; set; }
    }
}