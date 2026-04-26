using Auth0.AspNetCore.Authentication;
using BCT.Application.EventManagement.Notifiers;
using BCT.Application.Services;
using BCT.Blazor;
using BCT.Blazor.Components;
using BCT.Blazor.Services;
using BCT.Blazor.State;
using BCT.Infrastructure;
using Ljbc1994.Blazor.IntersectionObserver;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Radzen;
using Serilog;
using Sqids;
using System.Globalization;

Microsoft.Playwright.Program.Main(new[] { "install" });

var culture = new CultureInfo("nl-NL");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);
var logger = builder.Logging.AddConsole();

var subUrl = builder.Configuration["Url:SubSite"];
var isDebugEnvironment = builder.Environment.IsDevelopment() && !(builder.Configuration["Environment:Overwrite"] == "true");

Console.WriteLine($"Is debug environment: {isDebugEnvironment}");

//Add auth0
if(!isDebugEnvironment)
{	
	builder.Services.AddAuth0WebAppAuthentication(options =>
	{
		options.Domain = builder.Configuration["Auth0:Domain"];
		options.ClientId = builder.Configuration["Auth0:ClientId"];
		options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
		options.CallbackPath = "/callback";
		options.Scope = "openid profile email";
	});
}
else
{
	// Bypass authorization in development
	builder.Services.AddAuthentication("DebugScheme")
		.AddScheme<AuthenticationSchemeOptions, DebugAuthenticationHandler>("DebugScheme", null);

	builder.Services.AddAuthorization(options =>
	{
		options.DefaultPolicy = new AuthorizationPolicyBuilder("DebugScheme")
			.RequireAssertion(_ => true) // Always authorize in development
			.Build();
	});
}

// Add services to the container.

builder.Services.AddIntersectionObserver();
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();
builder.Services.AddRadzenCookieThemeService(options =>
{
	options.Name = "BCTTheme"; // The name of the cookie
	options.Duration = TimeSpan.FromDays(365); // The duration of the cookie
});

var baseFolder = builder.Configuration["DB:BaseFolder"];
baseFolder = string.IsNullOrEmpty(baseFolder) ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) : baseFolder;
string dbPath = Path.Join(baseFolder, $"{builder.Configuration["DB:DBName"]}_logs");


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Polly", Serilog.Events.LogEventLevel.Error)
    .WriteTo.Console()
    .WriteTo.File(builder.Configuration["Logging:LogFile"], rollingInterval: RollingInterval.Day)
    .WriteTo.Sink(new InMemorySink(null))
    .WriteTo.SQLite(dbPath)
    .CreateLogger();

builder.Host.UseSerilog();



//Blazor state/events:
//These are circuit state values that are shared by reference between components.
//They are new() because they are generated for each circuit.
//They are only used in the blazor side
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<SessionTracker>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<CircuitHandler, MyCircuitHandler>();
builder.Services.AddScoped<CurrentUserState>();
builder.Services.AddScoped<SelectedCompanyState>();
builder.Services.AddScoped<SelectedProjectState>();
builder.Services.AddScoped<SidebarState>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectValueService, ProjectValueService>();
builder.Services.AddTransient<IHelpTextService, HelpTextService>();
builder.Services.AddSingleton<IPrinter, Printer>();

builder.Services.AddCascadingValue<CurrentUserState>((sp) => sp.GetService<CurrentUserState>());
builder.Services.AddCascadingValue<SelectedCompanyState>((sp) => sp.GetService<SelectedCompanyState>());
builder.Services.AddCascadingValue<SelectedProjectState>((sp) => sp.GetService<SelectedProjectState>());
builder.Services.AddCascadingValue<IAuthService>((sp) => sp.GetService<IAuthService>());
builder.Services.AddCascadingValue<SidebarState>((sp) =>
{
    var sidebarState = sp.GetService<SidebarState>();
    sidebarState.Value = new() { Value = true };
    return sidebarState;
});

//Domain events:
//These are global events that are triggered by the application and can be listened to by any component.
//They are drawn from the service provider where they may be registered as singletons.
builder.Services.AddCascadingValue<NewCompanyNotifier>((serviceProvider) => serviceProvider.GetService<NewCompanyNotifier>());
builder.Services.AddCascadingValue<UserAuthenticatedNotifier>((serviceProvider) => serviceProvider.GetService<UserAuthenticatedNotifier>());
builder.Services.AddCascadingValue<NewProjectNotifier>((serviceProvider) => serviceProvider.GetService<NewProjectNotifier>());
builder.Services.AddCascadingValue<ProjectContentUpdatedNotifier>((serviceProvider) => serviceProvider.GetService<ProjectContentUpdatedNotifier>());
builder.Services.AddCascadingValue<CompanyContentUpdatedNotifier>((serviceProvider) => serviceProvider.GetService<CompanyContentUpdatedNotifier>());
builder.Services.AddCascadingValue<CompanyRemovedNotifier>((serviceProvider) => serviceProvider.GetService<CompanyRemovedNotifier>());
builder.Services.AddCascadingValue<ProjectRemovedNotifier>((serviceProvider) => serviceProvider.GetService<ProjectRemovedNotifier>());

//Infrastructure:
builder.Services.AddInfrastructureServices(builder.Configuration);

//BCT.Blazor specific services:
builder.Services.AddScoped<SqidsEncoder<int>>();

var app = builder.Build();
app.UsePathBase($"/{subUrl}");

if(!isDebugEnvironment)
{
	// Configure the HTTP request pipeline.
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

if(!isDebugEnvironment)
{
	app.UseAuthentication();
	app.UseAuthorization();
}

app.MapGet("/Account/Login", async (HttpContext httpContext, string returnUrl = "/") =>
{
	var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
		.WithRedirectUri(returnUrl)
		.Build();

	authenticationProperties.IsPersistent = true;

	await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

app.MapPost("/Account/LogOut", async (HttpContext httpContext) =>
{
	await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme);
	await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});
app.UseInfrastructure();

BCT.Infrastructure.Setup.SetupInfrastructure(app.Services);

app.Run();