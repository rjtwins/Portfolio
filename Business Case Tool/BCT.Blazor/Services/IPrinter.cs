namespace BCT.Blazor.Services;

public interface IPrinter
{
    Task<byte[]> PrintDashboard(string baseUrl, string token);
}