namespace BCT.Application.EventManagement;

public class Unsubscriber<T> : IDisposable
{
	private HashSet<IObserver<T>> _observers;
	private IObserver<T> _observer;
	
	public Unsubscriber(HashSet<IObserver<T>> observers, IObserver<T> observer)
	{
		_observers = observers;
		_observer = observer;
	}
	
	public virtual void Dispose()
	{
		if(!_observers.Contains(_observer))
			return;
			
		_observers.Remove(_observer);
	}
}