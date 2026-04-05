using System.Runtime.InteropServices;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Macros;

public sealed class MacroPlayer
{
    private readonly LogService _log;
    private CancellationTokenSource? _cts;
    private bool _isPlaying;

    public bool IsPlaying => _isPlaying;
    public event Action<int, int>? ProgressChanged; // (current, total)

    public MacroPlayer(LogService log)
    {
        _log = log;
    }

    public async Task PlayAsync(MacroDefinition macro, CancellationToken ct = default)
    {
        _isPlaying = true;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var token = _cts.Token;

        try
        {
            var repeatCount = macro.RepeatEnabled ? macro.RepeatCount : 1;

            for (int r = 0; r < repeatCount && !token.IsCancellationRequested; r++)
            {
                for (int i = 0; i < macro.Actions.Count && !token.IsCancellationRequested; i++)
                {
                    var action = macro.Actions[i];

                    if (action.DelayMs > 0)
                        await Task.Delay(action.DelayMs, token);

                    ExecuteAction(action);
                    ProgressChanged?.Invoke(i + 1, macro.Actions.Count);
                }

                if (macro.RepeatEnabled && macro.RepeatDelayMs > 0 && r < repeatCount - 1)
                    await Task.Delay(macro.RepeatDelayMs, token);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _isPlaying = false;
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    private void ExecuteAction(MacroAction action)
    {
        switch (action.Type)
        {
            case MacroActionType.KeyDown:
                SendKey((ushort)action.Key, false);
                break;
            case MacroActionType.KeyUp:
                SendKey((ushort)action.Key, true);
                break;
            case MacroActionType.KeyPress:
                SendKey((ushort)action.Key, false);
                SendKey((ushort)action.Key, true);
                break;
            case MacroActionType.Delay:
                Thread.Sleep(action.DelayMs);
                break;
        }
    }

    private static void SendKey(ushort vk, bool keyUp)
    {
        var input = new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_KEYBOARD,
            u = new NativeMethods.INPUTUNION
            {
                ki = new NativeMethods.KEYBDINPUT
                {
                    wVk = vk,
                    dwFlags = keyUp ? NativeMethods.KEYEVENTF_KEYUP : 0u
                }
            }
        };

        NativeMethods.SendInput(1, new[] { input }, Marshal.SizeOf<NativeMethods.INPUT>());
    }
}
