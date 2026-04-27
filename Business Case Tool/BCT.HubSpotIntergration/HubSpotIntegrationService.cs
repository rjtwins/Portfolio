using BCT.Application.EventManagement.Events;
using BCT.Application.EventManagement.Notifiers;
using BCT.Application.ServiceInterfaces;
using BCT.Application.UseCases.Queries;
using BCT.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;

namespace BCT.HubSpotIntegration;

public class HubSpotIntegrationService : IObserver<UserLoginEvent>, IObserver<UserLogoutEvent>, IHupSpotIntegrationService
{
    private readonly ILogger<EventLogger> logger;
    private readonly UserLoginNotifier userLoginNotifier;
    private readonly UserLogoutNotifier userLogoutNotifier;
    private readonly IGetUserUseCase getUserUseCase;
    private readonly IGetUsersUseCase getUsersUseCase;
    private readonly IGetUserRolesUseCase getUserRolesUseCase;
    private readonly IConfiguration configuration;

    public HubSpotIntegrationService(
        ILogger<EventLogger> logger,
        UserLoginNotifier userLoginNotifier, 
        UserLogoutNotifier userLogoutNotifier, 
        IGetUserUseCase getUserUseCase, 
        IGetUsersUseCase getUsersUseCase,
        IGetUserRolesUseCase getUserRolesUseCase,
        IConfiguration configuration)
    {
        this.logger = logger;
        this.userLoginNotifier = userLoginNotifier;
        this.userLogoutNotifier = userLogoutNotifier;
        this.getUserUseCase = getUserUseCase;
        this.getUsersUseCase = getUsersUseCase;
        this.getUserRolesUseCase = getUserRolesUseCase;
        this.configuration = configuration;
        userLoginNotifier.Subscribe(this);
        userLogoutNotifier.Subscribe(this);
    }

    public void OnCompleted() { }

    public void OnError(Exception error) { throw error; }

    public void OnNext(UserLoginEvent value)
    {
        UpdateHubSpot(value.userId, true);
    }

    public void OnNext(UserLogoutEvent value)
    {
        UpdateHubSpot(value.userId, false);
    }

    private async Task UpdateHubSpot(string userId, bool login)
    {

        if (configuration["HubSpot:Active"] != "true")
            return;

        if(!login)
            return;

        var users = await getUsersUseCase.ExecuteAsync();
        var user = users.FirstOrDefault(x => x.Name.ToLower() == userId.ToLower());

        if (user == null)
            return;

        var emailEncoded = Uri.EscapeDataString(user.Email);
        var roles = await getUserRolesUseCase.ExecuteAsync(user);
        var role = roles.Any(x => x.Name.ToLower() == "coach" || x.Name.ToLower() == "admin") ? "Coach" : "Ondernemer";

        var token = configuration["HubSpot:Token"];
        var client = new HttpClient();
        var propertiesList = string.Join(',', new List<string>
        {
            "aantal_keer_ingelogd",
            "laatste_keer_ingelogd",
            "email",
            "soort_rol"
        });


        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        var response = await client.GetAsync($"https://api.hubapi.com/crm/v3/objects/contacts/{user.Email.ToLower()}?idProperty=email&properties={propertiesList}");

        //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
        if (!response.IsSuccessStatusCode)
        {
            var json = JsonSerializer.Serialize(new
            {
                properties = new Properties
                {
                    aantal_keer_ingelogd = "1",
                    laatste_keer_ingelogd = DateTime.UtcNow,
                    email = user.Email,
                    soort_rol = role,
                }
            });

            var response2 = await client.PostAsync($"https://api.hubapi.com/crm/v3/objects/contacts/", new StringContent(json, Encoding.UTF8, "application/json"));

            return;
        }

        var text = await response.Content.ReadAsStringAsync();
        var jObj = JObject.Parse(text);
        string aantalKeerIngelogd = jObj["properties"]?["aantal_keer_ingelogd"]?.Value<string>() ?? "0";
        string hupSpotId = jObj["id"]?.Value<string>() ?? "";

        var json2 = JsonSerializer.Serialize(new
        {
            properties = new Properties
            {
                aantal_keer_ingelogd = (int.Parse(aantalKeerIngelogd) + 1).ToString(),
                laatste_keer_ingelogd = DateTime.UtcNow,
                email = user.Email,
                soort_rol = role,
            }
        });

        var response3 = await client.PatchAsync($"https://api.hubapi.com/crm/v3/objects/contacts/{hupSpotId}", new StringContent(json2, Encoding.UTF8, "application/json"));
    }

    public class Properties
    {
        public string aantal_keer_ingelogd { get; set; }
        public string email { get; set; }
        public DateTime laatste_keer_ingelogd { get; set; }
        public string soort_rol { get; set; }
    }
}
