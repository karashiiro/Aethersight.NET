using System;
using System.Collections.Generic;
using System.Diagnostics;
using AethersightNET.Packets;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AethersightNET
{
    public class AethersightSniffer : IAethersightSniffer
    {
        private readonly string tempExePath;
        private readonly Job job;

        private Process sniffProcess;
        private Process readFileProcess;

        public delegate void PacketCallback(
            string srcAddress,
            string dstAddress,
            FfxivArrPacketHeader packetHeader,
            FfxivArrSegmentHeader segmentHeader,
            FfxivArrIpcHeader ipcHeader,
            byte[] remainder);

        public AethersightSniffer()
        {
            this.job = new Job();

            this.tempExePath = Path.GetTempFileName();
            using var tempHandle = File.OpenWrite(this.tempExePath);
            using var executable = Assembly.GetExecutingAssembly().GetManifestResourceStream("AethersightNET.aethersight.exe")
                             ?? throw new FileNotFoundException("aethersight.exe not found in assembly!");
            executable.CopyTo(tempHandle);
            Trace.WriteLine($"Extracted executable to {this.tempExePath}");

            var zlib = Path.Combine(Path.GetTempPath(), "zlib1.dll");
            if (File.Exists(zlib))
            {
                try
                {
                    File.Delete(zlib);
                    Trace.WriteLine("Deleted existing zlib copy.");
                }
                catch (UnauthorizedAccessException)
                {
                    // Invalid Permissions
                    return;
                }
                catch (IOException)
                {
                    // File in use
                    return;
                }
            }

            using var zlibHandle = File.OpenWrite(zlib);
            using var lib = Assembly.GetExecutingAssembly().GetManifestResourceStream("AethersightNET.zlib1.dll")
                            ?? throw new FileNotFoundException("zlib1.dll not found in assembly!");
            lib.CopyTo(zlibHandle);
            Trace.WriteLine($"Extracted zlib to {zlib}");
        }

        public void BeginSniffing(PacketCallback callback, string deviceName = "")
        {
            if (this.sniffProcess != null) return;

            this.sniffProcess = Process.Start(new ProcessStartInfo
            {
                FileName = this.tempExePath,
                Arguments = !string.IsNullOrEmpty(deviceName) ? $"-d {deviceName}" : "",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
            this.sniffProcess.OutputDataReceived += (sender, e) => OutputDataReceived(callback, e);
            this.sniffProcess.BeginOutputReadLine();

            this.job.AddProcess(this.sniffProcess.Handle);
        }

        public void BeginSniffingFromFile(PacketCallback callback, string fileName)
        {
            if (this.readFileProcess == null) return;

            this.readFileProcess = Process.Start(new ProcessStartInfo
            {
                FileName = this.tempExePath,
                Arguments = !string.IsNullOrEmpty(fileName) ? $"-f {fileName}" : "",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
            this.readFileProcess.OutputDataReceived += (sender, e) => OutputDataReceived(callback, e);
            this.readFileProcess.BeginOutputReadLine();

            this.job.AddProcess(this.readFileProcess.Handle);
        }

        private static void OutputDataReceived(PacketCallback callback, DataReceivedEventArgs e)
        {
            var parameters = e.Data.Split(';')
                .Where(parameter => !string.IsNullOrEmpty(parameter))
                .Select(parameter =>
                {
                    var kvpRaw = parameter.Split('=');
                    return new KeyValuePair<string, string>(kvpRaw[0], kvpRaw[1]);
                })
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var srcAddress = parameters["src_address"];
            var dstAddress = parameters["dst_address"];

            var packetHeader = new FfxivArrPacketHeader
            {
                Unknown0 = ulong.Parse(parameters["unknown_0"]),
                Unknown8 = ulong.Parse(parameters["unknown_8"]),
                Timestamp = ulong.Parse(parameters["timestamp"]),
                Size = uint.Parse(parameters["total_size"]),
                ConnectionType = ushort.Parse(parameters["connection_type"]),
                SegmentCount = ushort.Parse(parameters["segment_count"]),
                Unknown20 = byte.Parse(parameters["unknown_20"]),
                IsCompressed = parameters["is_compressed"] == "true",
                Unknown24 = ushort.Parse(parameters["unknown_24"]),
            };

            var segmentHeader = new FfxivArrSegmentHeader
            {
                Size = uint.Parse(parameters["total_size"]),
                SourceActor = uint.Parse(parameters["source_actor"]),
                TargetActor = uint.Parse(parameters["target_actor"]),
                Type = (FfxivArrSegmentType) ushort.Parse(parameters["segment_type"]),
                Padding = 0,
            };

            FfxivArrIpcHeader ipcHeader = null;
            if (segmentHeader.Type == FfxivArrSegmentType.Ipc)
            {
                ipcHeader = new FfxivArrIpcHeader
                {
                    Reserved = 0x0014,
                    Type = ushort.Parse(parameters["ipc_type"]),
                    Padding = 0,
                    ServerId = ushort.Parse(parameters["server_id"]),
                    Timestamp = uint.Parse(parameters["ipc_timestamp"]),
                    Padding1 = 0,
                };
            }

            var remainder = parameters["remainder_data"].Split(' ')
                .Select(byte.Parse)
                .ToArray();

            callback(srcAddress, dstAddress, packetHeader, segmentHeader, ipcHeader, remainder);
        }

        public void EndSniffing()
        {
            if (this.sniffProcess != null) return;

            DisposeProcess(this.sniffProcess);
            this.sniffProcess = null;
        }

        public void EndSniffingFromFile()
        {
            if (this.readFileProcess == null) return;

            DisposeProcess(this.readFileProcess);
            this.readFileProcess = null;
        }

        private static void DisposeProcess(Process process)
        {
            if (process == null) return;

            try
            {
                process.CloseMainWindow();
                process.WaitForExit();
            }
            finally
            {
                process.Dispose();
            }
        }

        public void Dispose()
        {
            DisposeProcess(this.sniffProcess);
            DisposeProcess(this.readFileProcess);

            try
            {
                File.Delete(this.tempExePath);
            }
            catch
            {
                // ignored
            }

            this.job.Dispose();
        }
    }
}
