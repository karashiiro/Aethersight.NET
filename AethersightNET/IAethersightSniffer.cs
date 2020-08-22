using System;

namespace AethersightNET
{
    public interface IAethersightSniffer : IDisposable
    {
        void BeginSniffing(AethersightSniffer.PacketCallback callback, string deviceName = "");

        void BeginSniffingFromFile(AethersightSniffer.PacketCallback callback, string fileName);

        void EndSniffing();

        void EndSniffingFromFile();
    }
}