using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Macros;

public sealed class TriggerEngine : IDisposable
{
    private readonly KeybindManager _keybindManager;
    private readonly MacroPlayer _player;
    private readonly EventBus _eventBus;
    private readonly LogService _log;
    private readonly List<Action<object?>> _registeredHandlers = new();

    public TriggerEngine(KeybindManager keybindManager, MacroPlayer player, EventBus eventBus, LogService log)
    {
        _keybindManager = keybindManager;
        _player = player;
        _eventBus = eventBus;
        _log = log;
    }

    public void RegisterTriggers()
    {
        UnregisterAll();

        foreach (var macro in _keybindManager.Macros.Where(m => m.IsEnabled))
        {
            switch (macro.Trigger.Type)
            {
                case MacroTriggerType.OnGameLaunch:
                    RegisterEventTrigger(PluginSDK.Events.LauncherEvents.OnLaunch, macro);
                    break;
                case MacroTriggerType.OnFpsDrop:
                    RegisterEventTrigger(PluginSDK.Events.GameEvents.OnFpsDrop, macro);
                    break;
            }
        }
    }

    private void RegisterEventTrigger(string eventName, MacroDefinition macro)
    {
        Action<object?> handler = async _ =>
        {
            if (!_player.IsPlaying)
            {
                _log.Info("Trigger fired macro: {Name} (event: {Event})", macro.Name, eventName);
                await _player.PlayAsync(macro);
            }
        };

        _eventBus.Subscribe(eventName, handler);
        _registeredHandlers.Add(handler);
    }

    private void UnregisterAll()
    {
        _registeredHandlers.Clear();
    }

    public void Dispose()
    {
        UnregisterAll();
    }
}
