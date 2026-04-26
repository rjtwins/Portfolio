namespace BCT.Application.ServiceInterfaces;

public interface IAuth0ManagementTokenRefreshService
{
    public Task CheckAndRefresh();
}