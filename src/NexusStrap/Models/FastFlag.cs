using System.Text.Json.Serialization;

namespace NexusStrap.Models;

public sealed class FastFlag
{
    public string Name { get; set; } = string.Empty;
    public object? Value { get; set; }
    public FastFlagType Type { get; set; }
    public string Category { get; set; } = "General";
    public string? Description { get; set; }
    public bool IsCustom { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FastFlagType { Bool, Int, String, Double }

public sealed class FastFlagPreset
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public Dictionary<string, object> Flags { get; set; } = new();
}

public sealed class FastFlagSnapshot
{
    public DateTime Timestamp { get; set; }
    public string Label { get; set; } = string.Empty;
    public Dictionary<string, object?> Flags { get; set; } = new();
}
