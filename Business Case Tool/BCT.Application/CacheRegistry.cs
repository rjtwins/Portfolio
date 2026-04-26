using System.Timers;

namespace BCT.Application;

public class CacheRegistry : IObserver<ProjectContentUpdatedEvent>, IObserver<ProjectRemovedEvent>, ICacheRegistry
{
    //private readonly ProjectContentUpdatedNotifier projectContentUpdatedNotifier;
    //private readonly ProjectRemovedNotifier projectRemovedNotifier;
    private const int DefaultTimeout = 2000;


    public CacheRegistry(ProjectContentUpdatedNotifier projectContentUpdatedNotifier, ProjectRemovedNotifier projectRemovedNotifier)
    {
        //this.projectContentUpdatedNotifier = projectContentUpdatedNotifier;
        //this.projectRemovedNotifier = projectRemovedNotifier;

        projectRemovedNotifier.Subscribe(this);
        projectContentUpdatedNotifier.Subscribe(this);
    }

    private Dictionary<string, object> _cache { get; set; } = new();

    public bool TryGet(string key, out object value)
    {
        if (_cache.ContainsKey(key))
        {
            value = _cache[key];
            return true;
        }
        value = null;
        return false;
    }

    public object Get(string key)
    {
        if (_cache.ContainsKey(key))
        {
            return _cache[key];
        }
        return null;
    }

    public void Add(string key, object value, int? timeout = null)
    {
        _cache[key] = value;

        //Timeout
        var timer = new System.Timers.Timer()
        {
            AutoReset = false,
            Interval = timeout == null ? DefaultTimeout : (double)timeout.Value
        };
        timer.Elapsed += (object? sender, ElapsedEventArgs e) => Remove(key);
    }

    public void OnCompleted() { }

    public void OnError(Exception error) { }

    public void OnNext(ProjectContentUpdatedEvent value)
    {
        _cache
            .Where(x => x.Key.StartsWith(value.Project.Id.ToString()))
            .ToList()
            .ForEach(x => _cache.Remove(x.Key));
    }

    public void OnNext(ProjectRemovedEvent value)
    {
        _cache
            .Where(x => x.Key.StartsWith(value.ProjectId.ToString()))
            .ToList()
            .ForEach(x => _cache.Remove(x.Key));
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}
