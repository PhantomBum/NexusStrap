using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusStrap.Core.Stability;

namespace NexusStrap.UI.ViewModels;

public partial class StabilityViewModel : ObservableObject
{
    private readonly CrashLogger _crashLogger;
    private readonly DiagnosticsEngine _diagnostics;
    private readonly SafeMode _safeMode;
    private readonly DebugOverlay _debugOverlay;

    [ObservableProperty] private ObservableCollection<CrashReport> _crashReports = new();
    [ObservableProperty] private string _diagnosticsReport = "";
    [ObservableProperty] private bool _safeModeActive;
    [ObservableProperty] private bool _overlayActive;

    public StabilityViewModel(CrashLogger crashLogger, DiagnosticsEngine diagnostics,
        SafeMode safeMode, DebugOverlay debugOverlay)
    {
        _crashLogger = crashLogger;
        _diagnostics = diagnostics;
        _safeMode = safeMode;
        _debugOverlay = debugOverlay;

        SafeModeActive = safeMode.IsActive;
        CrashReports = new ObservableCollection<CrashReport>(crashLogger.GetRecentCrashes());
    }

    [RelayCommand]
    private async Task RunDiagnostics()
    {
        var report = await _diagnostics.RunDiagnosticsAsync();
        DiagnosticsReport = $"""
            OS: {report.OsVersion}
            .NET: {report.DotNetVersion}
            CPUs: {report.ProcessorCount}
            Memory: {report.TotalMemoryMb} MB
            Roblox Installed: {report.RobloxInstalled}
            Roblox Path: {report.RobloxPath ?? "N/A"}
            Roblox EXE: {report.RobloxExeExists}
            Protocol Registered: {report.ProtocolRegistered}
            NexusStrap Dir: {report.NexusStrapDir}
            Health: {(report.IsHealthy ? "HEALTHY" : "ISSUES FOUND")}
            """;
    }

    [RelayCommand]
    private void ToggleSafeMode()
    {
        if (_safeMode.IsActive) _safeMode.Deactivate(); else _safeMode.Activate();
        SafeModeActive = _safeMode.IsActive;
    }

    [RelayCommand]
    private void ToggleOverlay()
    {
        _debugOverlay.Toggle();
        OverlayActive = _debugOverlay.IsVisible;
    }

    [RelayCommand]
    private void RefreshCrashes()
    {
        CrashReports = new ObservableCollection<CrashReport>(_crashLogger.GetRecentCrashes());
    }

    [RelayCommand]
    private void OpenCrashReport(CrashReport report)
    {
        System.Diagnostics.Process.Start("notepad.exe", report.FilePath);
    }
}
