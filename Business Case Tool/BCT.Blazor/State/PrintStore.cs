using System.Collections.Concurrent;

namespace BCT.Blazor.State;

public class PrintStore
{
    private static ConcurrentDictionary<string, Dictionary<string, object>> PrintData { get; } = new();

    public static void Set(string key, Dictionary<string, object> data)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        }
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data), "Data cannot be null.");
        }

        // Ensure the key is unique and does not overwrite existing data
        if(!PrintData.TryAdd(key, data))
        {
            throw new InvalidOperationException($"A print data entry with the key '{key}' already exists.");
        }
    }

    public static Dictionary<string, object> Get(string key)
    {
        if(PrintData.TryGetValue(key, out var data))
        {
            return data;
        }

        //if (PrintData.TryRemove(key, out var data))
        //{
        //    return data;
        //}
        throw new KeyNotFoundException($"No print data found for key '{key}'.");
    }

    public static void Cleanup(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        }
        PrintData.TryRemove(key, out _);
    }
}
