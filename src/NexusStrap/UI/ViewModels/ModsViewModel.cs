using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusStrap.Core.Mods;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.UI.ViewModels;

public partial class ModsViewModel : ObservableObject
{
    private readonly ModManager _modManager;
    private readonly ModLoader _modLoader;

    [ObservableProperty] private ObservableCollection<ModInfo> _mods = new();
    [ObservableProperty] private ObservableCollection<ModConflict> _conflicts = new();
    [ObservableProperty] private string _statusText = "";

    public ModsViewModel(ModManager modManager, ModLoader modLoader)
    {
        _modManager = modManager;
        _modLoader = modLoader;
        _modManager.LoadMods();
        RefreshMods();
    }

    private void RefreshMods()
    {
        Mods = new ObservableCollection<ModInfo>(_modManager.Mods);
        Conflicts = new ObservableCollection<ModConflict>(_modManager.CheckConflicts());
        StatusText = $"{Mods.Count} mods loaded, {Mods.Count(m => m.IsEnabled)} enabled";
    }

    [RelayCommand]
    private void ToggleMod(ModInfo mod)
    {
        if (mod.IsEnabled)
            _modManager.DisableMod(mod.Id);
        else
            _modManager.EnableMod(mod.Id);
        RefreshMods();
    }

    [RelayCommand]
    private void InstallMod()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Mod Packages|*.zip" };
        if (dialog.ShowDialog() == true)
        {
            _modLoader.InstallFromZip(dialog.FileName);
            _modManager.LoadMods();
            RefreshMods();
        }
    }

    [RelayCommand]
    private void UninstallMod(ModInfo mod)
    {
        _modLoader.UninstallMod(mod);
        _modManager.LoadMods();
        RefreshMods();
    }

    [RelayCommand]
    private void ApplyMods()
    {
        _modManager.ApplyEnabledMods();
        StatusText = "Mods applied to Roblox installation";
    }
}
