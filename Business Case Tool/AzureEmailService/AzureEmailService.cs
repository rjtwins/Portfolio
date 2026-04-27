using BCT.Application.Dtos;
using BCT.Application.ServiceInterfaces;
using Newtonsoft.Json;
using Radzen;
using System.Net.Http.Json;
using System.Text;

namespace BCT.AzureEmailService;

public class AzureEmailService : IEmailService
{
    internal static string EmailLogicAppUrl;

    public async Task SendMail(Mail mail)
    {
        var json = JsonConvert.SerializeObject(mail);
        using var httpClient = new HttpClient();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(
            EmailLogicAppUrl,
            content
        );

        var message = await response.Content.ReadAsStringAsync();
    }
}
