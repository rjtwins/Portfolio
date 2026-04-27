namespace BCT.Application;

public interface ICacheRegistry
{
    void Add(string key, object value, int? timeout = null);
    bool TryGet(string key, out object value);
    object Get(string key);
    void OnCompleted();
    void OnError(Exception error);
    void OnNext(ProjectContentUpdatedEvent value);
    void OnNext(ProjectRemovedEvent value);
    void Remove(string key);
}