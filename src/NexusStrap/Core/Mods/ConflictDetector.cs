using NexusStrap.Models;

namespace NexusStrap.Core.Mods;

public sealed class ConflictDetector
{
    public IReadOnlyList<ModConflict> Detect(IReadOnlyList<ModInfo> mods)
    {
        var conflicts = new List<ModConflict>();
        var targetMap = new Dictionary<string, List<ModInfo>>(StringComparer.OrdinalIgnoreCase);

        foreach (var mod in mods.Where(m => m.IsEnabled))
        {
            foreach (var mapping in mod.FileMappings)
            {
                if (!targetMap.TryGetValue(mapping.Target, out var list))
                {
                    list = new List<ModInfo>();
                    targetMap[mapping.Target] = list;
                }
                list.Add(mod);
            }
        }

        foreach (var kvp in targetMap.Where(t => t.Value.Count > 1))
        {
            conflicts.Add(new ModConflict
            {
                TargetFile = kvp.Key,
                ConflictingMods = kvp.Value.Select(m => m.Name).ToList()
            });
        }

        return conflicts;
    }
}

public sealed class ModConflict
{
    public string TargetFile { get; set; } = string.Empty;
    public List<string> ConflictingMods { get; set; } = new();
}
