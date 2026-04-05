using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusStrap.Core.Macros;
using NexusStrap.Models;

namespace NexusStrap.UI.ViewModels;

public partial class MacrosViewModel : ObservableObject
{
    private readonly MacroRecorder _recorder;
    private readonly MacroPlayer _player;
    private readonly KeybindManager _keybindManager;

    [ObservableProperty] private ObservableCollection<MacroDefinition> _macros = new();
    [ObservableProperty] private bool _isRecording;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private string _recordingName = "";
    [ObservableProperty] private string _statusText = "";

    public MacrosViewModel(MacroRecorder recorder, MacroPlayer player, KeybindManager keybindManager)
    {
        _recorder = recorder;
        _player = player;
        _keybindManager = keybindManager;
        _keybindManager.LoadMacros();
        RefreshMacros();
    }

    private void RefreshMacros()
    {
        Macros = new ObservableCollection<MacroDefinition>(_keybindManager.Macros);
    }

    [RelayCommand]
    private void StartRecording()
    {
        _recorder.StartRecording();
        IsRecording = true;
        StatusText = "Recording... Press keys to record actions";
    }

    [RelayCommand]
    private void StopRecording()
    {
        var name = string.IsNullOrWhiteSpace(RecordingName) ? $"Macro {DateTime.Now:HHmmss}" : RecordingName;
        var macro = _recorder.StopRecording(name);
        _keybindManager.SaveMacro(macro);
        IsRecording = false;
        RecordingName = "";
        StatusText = $"Recorded: {macro.Name} ({macro.Actions.Count} actions)";
        RefreshMacros();
    }

    [RelayCommand]
    private async Task PlayMacro(MacroDefinition macro)
    {
        IsPlaying = true;
        StatusText = $"Playing: {macro.Name}...";
        await _player.PlayAsync(macro);
        IsPlaying = false;
        StatusText = $"Finished: {macro.Name}";
    }

    [RelayCommand]
    private void StopPlaying() => _player.Stop();

    [RelayCommand]
    private void DeleteMacro(MacroDefinition macro)
    {
        _keybindManager.DeleteMacro(macro.Id);
        RefreshMacros();
    }

    [RelayCommand]
    private void ToggleMacro(MacroDefinition macro)
    {
        macro.IsEnabled = !macro.IsEnabled;
        _keybindManager.SaveMacro(macro);
    }
}
