# Aethersight.NET
Windows-only .NET wrapper for the [Aethersight](https://github.com/karashiiro/Aethersight) CLI.

## Example
```csharp
using var aethersight = new AethersightSniffer();

aethersight.BeginSniffing((srcAddress, dstAddress, packetHeader, segmentHeader, ipcHeader, ipcData) =>
{
    if (ipcHeader != null)
    {
        Console.WriteLine($"Got opcode {ipcHeader.Type}!");
    }
});
```

## Remarks
This wrapper extracts a bundled copy of the CLI and its zlib dependency to your system's temporary directory and runs it. This will likely trigger your antivirus,
but the scan should pass without any errors.

The wrapper uses the [Job objects](https://docs.microsoft.com/en-us/windows/win32/procthread/job-objects) API to close its spawned processes after termination,
coupling it somewhat tightly to the Windows API. It would've been much cleaner to simply P/Invoke the exported methods from a shared library, but I couldn't
write a valid managed callback signature after some time and I went with this approach instead.