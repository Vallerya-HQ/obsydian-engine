namespace Obsydian.Core.Logging;

public enum LogLevel
{
    Trace,
    Debug,
    Info,
    Warn,
    Error,
    Fatal
}

/// <summary>
/// Simple, fast logger for engine diagnostics.
/// </summary>
public static class Log
{
    public static LogLevel MinLevel { get; set; } = LogLevel.Debug;
    public static Action<LogLevel, string, string>? OnLog { get; set; }

    public static void Trace(string tag, string message) => Write(LogLevel.Trace, tag, message);
    public static void Debug(string tag, string message) => Write(LogLevel.Debug, tag, message);
    public static void Info(string tag, string message) => Write(LogLevel.Info, tag, message);
    public static void Warn(string tag, string message) => Write(LogLevel.Warn, tag, message);
    public static void Error(string tag, string message) => Write(LogLevel.Error, tag, message);
    public static void Fatal(string tag, string message) => Write(LogLevel.Fatal, tag, message);

    private static void Write(LogLevel level, string tag, string message)
    {
        if (level < MinLevel) return;

        if (OnLog is not null)
        {
            OnLog(level, tag, message);
            return;
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        Console.WriteLine($"[{timestamp}] [{level}] [{tag}] {message}");
    }
}
