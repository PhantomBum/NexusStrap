using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusStrap.Core.FastFlags;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.UI.ViewModels;

public partial class FastFlagViewModel : ObservableObject
{
    private readonly FastFlagManager _flagManager;
    private readonly SettingsService _settings;

    [ObservableProperty] private string _searchQuery = "";
    [ObservableProperty] private string _newFlagName = "";
    [ObservableProperty] private string _newFlagValue = "";
    [ObservableProperty] private ObservableCollection<FastFlagEntry> _flags = new();
    [ObservableProperty] private ObservableCollection<FastFlagSnapshot> _history = new();

    public IReadOnlyList<FastFlagPreset> Presets => FastFlagPresets.BuiltInPresets;

    public FastFlagViewModel(FastFlagManager flagManager, SettingsService settings)
    {
        _flagManager = flagManager;
        _settings = settings;

        _flagManager.LoadFlags();
        _flagManager.History.Load();
        RefreshFlags();
        RefreshHistory();
    }

    private void RefreshFlags()
    {
        Flags.Clear();
        var query = SearchQuery?.ToLowerInvariant() ?? "";
        foreach (var kvp in _flagManager.CurrentFlags)
        {
            if (string.IsNullOrEmpty(query) || kvp.Key.ToLowerInvariant().Contains(query))
            {
                Flags.Add(new FastFlagEntry { Name = kvp.Key, Value = kvp.Value?.ToString() ?? "" });
            }
        }
    }

    private void RefreshHistory()
    {
        History = new ObservableCollection<FastFlagSnapshot>(_flagManager.History.Snapshots);
    }

    partial void OnSearchQueryChanged(string value) => RefreshFlags();

    [RelayCommand]
    private void AddFlag()
    {
        if (string.IsNullOrWhiteSpace(NewFlagName)) return;
        _flagManager.SetFlag(NewFlagName, ParseValue(NewFlagValue));
        NewFlagName = "";
        NewFlagValue = "";
        RefreshFlags();
    }

    [RelayCommand]
    private void RemoveFlag(string name)
    {
        _flagManager.RemoveFlag(name);
        RefreshFlags();
    }

    [RelayCommand]
    private void ApplyPreset(FastFlagPreset preset)
    {
        _flagManager.ApplyPreset(preset);
        RefreshFlags();
        RefreshHistory();
    }

    [RelayCommand]
    private void SaveSnapshot()
    {
        _flagManager.History.SaveSnapshot("Manual save", _flagManager.CurrentFlags);
        RefreshHistory();
    }

    [RelayCommand]
    private void RestoreSnapshot(int index)
    {
        var flags = _flagManager.History.GetSnapshot(index);
        if (flags is null) return;

        _flagManager.ClearFlags();
        foreach (var kvp in flags)
            _flagManager.SetFlag(kvp.Key, kvp.Value);
        RefreshFlags();
    }

    [RelayCommand]
    private void ImportFlags()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON Files|*.json|NexusStrap Preset|*.nexuspreset|All Files|*.*"
        };
        if (dialog.ShowDialog() == true)
        {
            _flagManager.ImportFlags(dialog.FileName);
            RefreshFlags();
        }
    }

    [RelayCommand]
    private void ExportFlags()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "NexusStrap Preset|*.nexuspreset|JSON Files|*.json",
            FileName = "flags_export"
        };
        if (dialog.ShowDialog() == true)
            _flagManager.ExportFlags(dialog.FileName);
    }

    [RelayCommand]
    private void ClearAll()
    {
        _flagManager.History.SaveSnapshot("Before clear", _flagManager.CurrentFlags);
        _flagManager.ClearFlags();
        RefreshFlags();
        RefreshHistory();
    }

    private static object? ParseValue(string value)
    {
        if (bool.TryParse(value, out var b)) return b;
        if (int.TryParse(value, out var i)) return i;
        if (double.TryParse(value, out var d)) return d;
        return value;
    }
}

public sealed class FastFlagEntry
{
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
}
