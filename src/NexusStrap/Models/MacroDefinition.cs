using System.Text.Json.Serialization;

namespace NexusStrap.Models;

public sealed class MacroDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string? GameId { get; set; }
    public MacroTrigger Trigger { get; set; } = new();
    public List<MacroAction> Actions { get; set; } = new();
    public bool IsEnabled { get; set; }
    public bool RepeatEnabled { get; set; }
    public int RepeatCount { get; set; } = 1;
    public int RepeatDelayMs { get; set; } = 100;
}

public sealed class MacroTrigger
{
    public MacroTriggerType Type { get; set; } = MacroTriggerType.Hotkey;
    public string? Hotkey { get; set; }
    public int? FpsThreshold { get; set; }
    public string? EventName { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MacroTriggerType { Hotkey, OnGameLaunch, OnFpsDrop, OnTimer }

public sealed class MacroAction
{
    public MacroActionType Type { get; set; }
    public int Key { get; set; }
    public int DelayMs { get; set; }
    public bool IsKeyDown { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MacroActionType { KeyDown, KeyUp, KeyPress, Delay, MouseClick, MouseMove }
