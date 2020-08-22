using System;
using System.Text;
using AethersightNET;

namespace AethersightConsoleTest
{
    public class Program
    {
        public static void Main()
        {
            using var aethersight = new AethersightSniffer();

            aethersight.BeginSniffing((srcAddress, dstAddress, packetHeader, segmentHeader, ipcHeader, ipcData) =>
            {
                var sb = new StringBuilder();
                sb.Append("src_address=").Append(srcAddress).Append(";");
                sb.Append("dst_address=").Append(dstAddress).Append(";");

                sb.Append("unknown_0=").Append(packetHeader.Unknown0).Append(";");
                sb.Append("unknown_8=").Append(packetHeader.Unknown8).Append(";");
                sb.Append("timestamp=").Append(packetHeader.Timestamp).Append(";");
                sb.Append("total_size=").Append(packetHeader.Size).Append(";");
                sb.Append("connection_type=").Append(packetHeader.ConnectionType).Append(";");
                sb.Append("segment_count=").Append(packetHeader.SegmentCount).Append(";");
                sb.Append("unknown_20=").Append(packetHeader.Unknown20).Append(";");
                sb.Append("is_compressed=").Append(packetHeader.IsCompressed).Append(";");
                sb.Append("unknown_24=").Append(packetHeader.Unknown24).Append(";");

                sb.Append("segment_size=").Append(segmentHeader.Size).Append(";");
                sb.Append("source_actor=").Append(segmentHeader.SourceActor).Append(";");
                sb.Append("target_actor=").Append(segmentHeader.TargetActor).Append(";");
                sb.Append("segment_type=").Append((ushort)segmentHeader.Type).Append(";");

                if (ipcHeader != null)
                {
                    sb.Append("ipc_type=").Append(ipcHeader.Type).Append(";");
                    sb.Append("server_id=").Append(ipcHeader.ServerId).Append(";");
                    sb.Append("ipc_timestamp=").Append(ipcHeader.Timestamp).Append(";");
                }

                sb.Append("remainder_data=").Append(string.Join(' ', ipcData)).Append(";");

                sb.AppendLine();

                Console.Write(sb.ToString());
            });
        }
    }
}
