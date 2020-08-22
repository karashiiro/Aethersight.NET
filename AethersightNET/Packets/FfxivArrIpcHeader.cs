namespace AethersightNET.Packets
{
    public class FfxivArrIpcHeader
    {
        public ushort Reserved;
        public ushort Type;
        public ushort Padding;
        public ushort ServerId;
        public uint Timestamp;
        public uint Padding1;
    }
}