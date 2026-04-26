namespace BCT.Application.Services;

public interface IUserSyncService
{
	public void Start();
	public Task SyncUsers();
}