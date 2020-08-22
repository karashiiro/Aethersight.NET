namespace AethersightNET.Packets
{
    public class FfxivArrSegmentHeader
    {
        /// <summary>
        /// The size of the segment header and its data.
        /// </summary>
        public uint Size;

        /// <summary>
        /// The session ID this segment describes.
        /// </summary>
        public uint SourceActor;

        /// <summary>
        /// The session ID this packet is being delivered to.
        /// </summary>
        public uint TargetActor;

        /// <summary>
        /// The segment type. (1, 2, 3, 7, 8, 9, 10)
        /// </summary>
        public FfxivArrSegmentType Type;

        public ushort Padding;
    }
}