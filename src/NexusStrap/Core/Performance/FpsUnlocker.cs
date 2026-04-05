using System.Diagnostics;
using System.Runtime.InteropServices;
using NexusStrap.Services;

namespace NexusStrap.Core.Performance;

public sealed class FpsUnlocker : IDisposable
{
    private readonly LogService _log;
    private Timer? _scanTimer;
    private int _targetFps;
    private bool _isActive;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
    private const uint PROCESS_VM_READ = 0x0010;
    private const uint PROCESS_VM_WRITE = 0x0020;
    private const uint PROCESS_VM_OPERATION = 0x0008;

    public bool IsActive => _isActive;
    public int TargetFps => _targetFps;

    public FpsUnlocker(LogService log)
    {
        _log = log;
    }

    public void Start(int targetFps = 0)
    {
        _targetFps = targetFps;
        _isActive = true;
        _scanTimer = new Timer(ScanAndPatch, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        _log.Info("FPS Unlocker started (target: {Fps})", targetFps == 0 ? "unlimited" : targetFps.ToString());
    }

    public void Stop()
    {
        _isActive = false;
        _scanTimer?.Dispose();
        _scanTimer = null;
        _log.Info("FPS Unlocker stopped");
    }

    public void SetTargetFps(int fps)
    {
        _targetFps = fps;
        _log.Info("FPS cap set to {Fps}", fps == 0 ? "unlimited" : fps.ToString());
    }

    private void ScanAndPatch(object? state)
    {
        if (!_isActive) return;

        try
        {
            var processes = Process.GetProcessesByName("RobloxPlayerBeta");
            foreach (var process in processes)
            {
                try
                {
                    PatchFpsLimit(process);
                }
                catch (Exception ex)
                {
                    _log.Debug("FPS patch attempt failed for PID {Pid}: {Message}", process.Id, ex.Message);
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "FPS Unlocker scan failed");
        }
    }

    private void PatchFpsLimit(Process process)
    {
        var handle = OpenProcess(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, false, process.Id);
        if (handle == IntPtr.Zero) return;

        try
        {
            var targetValue = _targetFps <= 0 ? 999999.0 : (double)_targetFps;
            var targetBytes = BitConverter.GetBytes(targetValue);

            // Scan for the frame rate limiter value (typically 60.0 double)
            // The Roblox client stores FPS cap as a double value in memory
            var module = process.MainModule;
            if (module is null) return;

            var baseAddress = module.BaseAddress;
            var moduleSize = module.ModuleMemorySize;

            var knownFpsCaps = new[] { 30.0, 60.0, 120.0, 144.0, 165.0, 240.0 };

            foreach (var cap in knownFpsCaps)
            {
                var scanBytes = BitConverter.GetBytes(cap);
                var addresses = ScanMemory(handle, baseAddress, moduleSize, scanBytes);

                foreach (var addr in addresses)
                {
                    WriteProcessMemory(handle, addr, targetBytes, targetBytes.Length, out _);
                }
            }
        }
        finally
        {
            CloseHandle(handle);
        }
    }

    private static List<IntPtr> ScanMemory(IntPtr processHandle, IntPtr baseAddress, int size, byte[] pattern)
    {
        var results = new List<IntPtr>();
        var buffer = new byte[4096];
        var offset = 0;

        while (offset < size)
        {
            var readSize = Math.Min(buffer.Length, size - offset);
            if (!ReadProcessMemory(processHandle, baseAddress + offset, buffer, readSize, out var bytesRead) || bytesRead == 0)
            {
                offset += buffer.Length;
                continue;
            }

            for (int i = 0; i <= bytesRead - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (buffer[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    results.Add(baseAddress + offset + i);
                }
            }

            offset += bytesRead;
        }

        return results;
    }

    public void Dispose()
    {
        Stop();
    }
}
