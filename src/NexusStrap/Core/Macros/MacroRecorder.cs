using System.Diagnostics;
using System.Runtime.InteropServices;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Macros;

public sealed class MacroRecorder : IDisposable
{
    private readonly LogService _log;
    private IntPtr _hookId = IntPtr.Zero;
    private NativeMethods.LowLevelKeyboardProc? _hookProc;
    private readonly List<MacroAction> _recordedActions = new();
    private readonly Stopwatch _stopwatch = new();
    private bool _isRecording;

    public bool IsRecording => _isRecording;
    public IReadOnlyList<MacroAction> RecordedActions => _recordedActions;

    public event Action<MacroAction>? ActionRecorded;

    public MacroRecorder(LogService log)
    {
        _log = log;
    }

    public void StartRecording()
    {
        if (_isRecording) return;

        _recordedActions.Clear();
        _isRecording = true;
        _stopwatch.Restart();

        _hookProc = HookCallback;
        _hookId = NativeMethods.SetHook(_hookProc);
        _log.Info("Macro recording started");
    }

    public MacroDefinition StopRecording(string name)
    {
        _isRecording = false;
        _stopwatch.Stop();

        if (_hookId != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        var macro = new MacroDefinition
        {
            Name = name,
            Actions = new List<MacroAction>(_recordedActions)
        };

        _log.Info("Macro recording stopped: {Name} ({Count} actions)", name, _recordedActions.Count);
        return macro;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _isRecording)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            var wParamInt = wParam.ToInt32();

            var action = new MacroAction
            {
                Key = vkCode,
                DelayMs = (int)_stopwatch.ElapsedMilliseconds,
                Type = wParamInt == NativeMethods.WM_KEYDOWN ? MacroActionType.KeyDown : MacroActionType.KeyUp,
                IsKeyDown = wParamInt == NativeMethods.WM_KEYDOWN
            };

            _recordedActions.Add(action);
            _stopwatch.Restart();
            ActionRecorded?.Invoke(action);
        }

        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (_hookId != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }
}

internal static class NativeMethods
{
    public const int WH_KEYBOARD_LL = 13;
    public const int WM_KEYDOWN = 0x0100;
    public const int WM_KEYUP = 0x0101;

    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetModuleHandle(string? lpModuleName);

    public static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint type;
        public INPUTUNION u;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUTUNION
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    public const uint INPUT_KEYBOARD = 1;
    public const uint KEYEVENTF_KEYUP = 0x0002;
}
