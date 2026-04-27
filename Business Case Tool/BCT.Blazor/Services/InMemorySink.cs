namespace BCT.Blazor.Services;

using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;

public class InMemorySink : ILogEventSink
{
    private readonly IFormatProvider _formatProvider;
    public static readonly ConcurrentQueue<string> Logs = new();

    public InMemorySink(IFormatProvider formatProvider)
    {
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        var renderedMessage = logEvent.RenderMessage(_formatProvider);
        Logs.Enqueue($"[{logEvent.Timestamp}][{logEvent.Level}] {renderedMessage}");

        // Cap size to avoid memory bloat
        while (Logs.Count > 1000)
            Logs.TryDequeue(out _);
    }
}