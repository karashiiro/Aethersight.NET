namespace AethersightNET.Packets
{
    public enum FfxivArrSegmentType : ushort
    {
        SessionInit = 1,
        Ipc = 3,
        KeepAlive = 7,
        //Response = 8,
        EncryptionInit = 9,
    }
}