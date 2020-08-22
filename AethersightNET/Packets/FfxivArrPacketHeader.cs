namespace AethersightNET.Packets
{
    public class FfxivArrPacketHeader
    {
        public ulong Unknown0;

        public ulong Unknown8;

        /// <summary>
        /// Represents the number of milliseconds since epoch that the packet was sent.
        /// </summary>
        public ulong Timestamp;

        /// <summary>
        /// The size of the packet header and its payload.
        /// </summary>
        public uint Size;

        /// <summary>
        /// The type of this connection - 1 zone, 2 chat.
        /// </summary>
        public ushort ConnectionType;

        /// <summary>
        /// The number of packet segments that follow.
        /// </summary>
        public ushort SegmentCount;

        public byte Unknown20;

        /// <summary>
        /// Indicates if the data segments of this packet are compressed.
        /// </summary>
        public bool IsCompressed;

        public uint Unknown24;
    }
}