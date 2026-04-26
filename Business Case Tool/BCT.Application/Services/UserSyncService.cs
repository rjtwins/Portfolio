namespace BCT.Application.Services;

public class UserSyncService : IObserver<UserAuthenticatedEvent>, IUserSyncService, IDisposable
{
	private readonly IAuthManagementService authManagementService;
	private readonly IRepository<User> userRepository;
	private readonly UserAuthenticatedNotifier userAuthenticatedObserver;

	private IDisposable? unsubscriber {get; set;}
	
	public UserSyncService(
		IAuthManagementService authManagementService, 
		IRepository<User> userRepository,
        UserAuthenticatedNotifier userAuthenticatedObserver)
	{
		this.authManagementService = authManagementService;
		this.userRepository = userRepository;
		this.userAuthenticatedObserver = userAuthenticatedObserver;
	}
	
	public void Start()
	{
		if(unsubscriber != null)
		{
			unsubscriber.Dispose();
		}
		
		unsubscriber = userAuthenticatedObserver.Subscribe(this);
	}
	
	public async Task SyncUsers()
	{
		var authUsers = await authManagementService.GetAllUsers();
		var users = await userRepository.GetAll();
		
		var missingLocalUsers = authUsers.Where(x => !(users.Select(y => y.AuthId).Contains(x.user_id)));
			
		var tasks = missingLocalUsers.Select(async x => 
		{
			var newLocalUser = new User
			{
				Name = x.email,
				AuthId = x.user_id,
				Email = x.email,
			};
			await userRepository.Add(newLocalUser);
		});
		
		await Task.WhenAll(tasks);
	}

	public void OnCompleted() => unsubscriber?.Dispose();
	public void OnError(Exception error) => throw error;
	public void OnNext(UserAuthenticatedEvent value) => SyncUsers();
	
	public void Dispose()
	{
		unsubscriber?.Dispose();
	}
}
