using BCT.Application.EventManagement.Events;
using BCT.Application.EventManagement.Notifiers;
using System.Collections.Concurrent;
using System.Timers;

public class SessionTracker
{
    private readonly UserLoginNotifier userLoginNotifier;
    private readonly UserLogoutNotifier userLogoutNotifier;

    private ConcurrentDictionary<string, int> _userSessions = new();
    private ConcurrentBag<string> _activeUsers = new();

    public SessionTracker(UserLoginNotifier userLoginNotifier, UserLogoutNotifier userLogoutNotifier)
    {
        this.userLoginNotifier = userLoginNotifier;
        this.userLogoutNotifier = userLogoutNotifier;
    }

    public Dictionary<string, int> GetCurrentSessions()
    {
        return _userSessions.ToDictionary<string, int>();
    }

    public void UserStartedSession(string? userId)
    {
        if (userId == null)
            return;

        if (!_activeUsers.Contains(userId))
        {
            _activeUsers.Add(userId);
            OnUserLoggedIn(userId);
        }

        var count = _userSessions.AddOrUpdate(userId, 1, (key, oldValue) => oldValue + 1);
    }

    public void UserStoppedSession(string? userId)
    {
        if (userId == null)
            return;

        var t = new System.Timers.Timer();
        t.Interval = 5000;
        t.AutoReset = false;
        t.Elapsed += (object? sender, ElapsedEventArgs e) =>
        {
            if (_userSessions.TryGetValue(userId, out int count))
            {
                count--;
                if (count <= 0)
                {
                    _userSessions.TryRemove(userId, out _);
                    _activeUsers.TryTake(out userId);
                    OnUserLoggedOut(userId);
                }
                else
                    _userSessions[userId] = count;
            }

            t.Dispose();
        };

        t.Start();
    }

    private void OnUserLoggedIn(string userId)
    {
        userLoginNotifier.Notify(new UserLoginEvent(userId));
    }

    private void OnUserLoggedOut(string userId)
    {
        userLogoutNotifier.Notify(new UserLogoutEvent(userId));
    }
}