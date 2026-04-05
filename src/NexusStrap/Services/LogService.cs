using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace NexusStrap.Services;

public sealed class LogService : IDisposable
{
    private readonly ILogger _logger;

    public LogService(string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);

        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.File(
                Path.Combine(logDirectory, "nexusstrap-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    public void Info(string message, params object[] args) => _logger.Information(message, args);
    public void Debug(string message, params object[] args) => _logger.Debug(message, args);
    public void Warning(string message, params object[] args) => _logger.Warning(message, args);
    public void Error(string message, params object[] args) => _logger.Error(message, args);
    public void Error(Exception ex, string message, params object[] args) => _logger.Error(ex, message, args);
    public void Fatal(string message, params object[] args) => _logger.Fatal(message, args);
    public void Fatal(Exception ex, string message, params object[] args) => _logger.Fatal(ex, message, args);

    public ILogger ForContext(string source) => _logger.ForContext("SourceContext", source);

    public void Dispose()
    {
        (Log.Logger as IDisposable)?.Dispose();
    }
}
