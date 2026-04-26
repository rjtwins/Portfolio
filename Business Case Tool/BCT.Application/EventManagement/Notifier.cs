namespace BCT.Application.EventManagement;

public abstract class Notifier<T> : IObservable<T>, IDisposable
{
	protected HashSet<IObserver<T>> _observers {get; set;} = new();
	
	public virtual void Notify(T value)
	{
		_observers.RemoveWhere(x => x == null);
		_observers.ToList().ForEach(x => NotifyObserversSave(x, value));
    }

    private void NotifyObserversSave(IObserver<T> observer, T value)
    {
       Task.Factory.StartNew(() => observer.OnNext(value));
    }

	public virtual IDisposable Subscribe(IObserver<T> observer)
	{
		_observers.Add(observer);
		return new Unsubscriber<T>(_observers, observer);
	}
	
	public virtual void Unsubscribe(IObserver<T> observer)
	{
		_observers.Remove(observer);
	}

	public void Dispose()
	{
		_observers.ToList().ForEach(x => x.OnCompleted());
		_observers.Clear();
	}
}
