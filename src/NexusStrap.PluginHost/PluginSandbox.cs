using System.Reflection;
using System.Runtime.Loader;
using NexusStrap.PluginSDK;

namespace NexusStrap.PluginHost;

public sealed class PluginSandbox
{
    private PluginAssemblyLoadContext? _context;
    private WeakReference? _contextRef;

    public string DllPath { get; }
    public IPlugin? Plugin { get; private set; }
    public bool IsLoaded => _context is not null;

    public PluginSandbox(string dllPath)
    {
        DllPath = dllPath;
    }

    public IPlugin? LoadAndCreate()
    {
        _context = new PluginAssemblyLoadContext(DllPath);
        _contextRef = new WeakReference(_context, trackResurrection: true);

        var assembly = _context.LoadFromAssemblyPath(DllPath);
        var pluginType = assembly.GetTypes()
            .FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

        if (pluginType is null) return null;

        Plugin = (IPlugin?)Activator.CreateInstance(pluginType);
        return Plugin;
    }

    public void Unload()
    {
        Plugin = null;
        _context?.Unload();
        _context = null;
    }

    public bool IsCollected => _contextRef is not null && !_contextRef.IsAlive;

    private sealed class PluginAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public PluginAssemblyLoadContext(string pluginPath) : base(isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var path = _resolver.ResolveAssemblyToPath(assemblyName);
            return path is not null ? LoadFromAssemblyPath(path) : null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            return path is not null ? LoadUnmanagedDllFromPath(path) : IntPtr.Zero;
        }
    }
}
