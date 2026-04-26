using BCT.Application.EventManagement.Events;
using BCT.Application.EventManagement.Notifiers;
using BCT.Application.Services;

namespace BCT.Blazor.Services;


/// <summary>
/// THIS SERIVE MUST BE SCOPED TO THE CIRCUIT
/// </summary>
public class CurrentUserService : ICurrentUserService, IObserver<UserLoginEvent>
{
    private string _currentUserId { get; set; } = string.Empty;

    public CurrentUserService(UserLoginNotifier userLoginNotifier)
    {
        userLoginNotifier.Subscribe(this);
    }

    public string GetCurrentUserUserId()
    {
        return _currentUserId;
    }

    public void OnCompleted() { }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnNext(UserLoginEvent value)
    {
        _currentUserId = value.userId;
    }
}
