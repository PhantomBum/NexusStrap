namespace NexusStrap.PluginSDK;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PluginAttribute : Attribute
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string MinHostVersion { get; set; } = "1.0.0";

    public PluginAttribute(string id, string name)
    {
        Id = id;
        Name = name;
    }
}
