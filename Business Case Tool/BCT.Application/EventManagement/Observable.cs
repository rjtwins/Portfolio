namespace BCT.Application.EventManagement;

public abstract class Observable<T> : IObservable<T>, IDisposable
{
	protected HashSet<IObserver<T>> _observers {get; set;} = new();
	
	public T Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
			Notify(value);
		}
	}
	protected T _value {get; set;}
			
	protected virtual void Notify(T value)
	{
		_observers.RemoveWhere(x => x == null);
		_observers.ToList().ForEach(x =>
        {
            Task.Factory.StartNew(() => x.OnNext(value));
        });
	}

	public IDisposable Subscribe(IObserver<T> observer)
	{
		_observers.Add(observer);
		return new Unsubscriber<T>(_observers, observer);
	}
	
	public void Unsubscribe(IObserver<T> observer)
	{
		_observers.Remove(observer);
	}

	public void Dispose()
	{
		_observers.ToList().ForEach(x => x.OnCompleted());
		_observers.Clear();
	}
}