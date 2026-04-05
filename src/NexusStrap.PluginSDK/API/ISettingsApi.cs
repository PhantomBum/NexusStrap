namespace NexusStrap.PluginSDK.API;

public interface ISettingsApi
{
    T? GetValue<T>(string key);
    void SetValue<T>(string key, T value);
    bool HasKey(string key);
    void RemoveKey(string key);
    void Save();
}
