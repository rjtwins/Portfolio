using BCT.Application.EventManagement.Events;
using BCT.Application.EventManagement.Notifiers;
using BCT.Application.ServiceInterfaces;
using Microsoft.Extensions.Logging;

namespace BCT.Logging;

public class EventLogger : 
    IEventLogger,
    IObserver<NewCompanyEvent>, 
    IObserver<NewProjectEvent>, 
    IObserver<ProjectRemovedEvent>, 
    IObserver<CompanyRemovedEvent>,
    IObserver<UserLoginEvent>, 
    IObserver<UserLogoutEvent>,
    IObserver<ProjectContentUpdatedEvent>,
    IObserver<CompanyContentUpdatedEvent>,
    IDisposable
{
    private readonly ILogger<EventLogger> logger;
    private readonly NewProjectNotifier newProjectNotifier;
    private readonly NewCompanyNotifier newCompanyNotifier;
    private readonly ProjectRemovedNotifier projectRemovedNotifier;
    private readonly CompanyRemovedNotifier companyRemovedNotifier;
    private readonly UserLoginNotifier userLoginNotifier;
    private readonly UserLogoutNotifier userLogoutNotifier;
    private readonly ProjectContentUpdatedNotifier projectContentUpdatedNotifier;
    private readonly CompanyContentUpdatedNotifier companyContentUpdatedNotifier;

    public EventLogger(
        ILogger<EventLogger> logger,
        NewProjectNotifier newProjectNotifier, 
        NewCompanyNotifier newCompanyNotifier,
        ProjectRemovedNotifier projectRemovedNotifier, 
        CompanyRemovedNotifier companyRemovedNotifier,
        UserLoginNotifier userLoginNotifier,
        UserLogoutNotifier userLogoutNotifier,
        ProjectContentUpdatedNotifier projectContentUpdatedNotifier,
        CompanyContentUpdatedNotifier companyContentUpdatedNotifier)
    {
        this.logger = logger;
        this.newProjectNotifier = newProjectNotifier;
        this.newCompanyNotifier = newCompanyNotifier;
        this.projectRemovedNotifier = projectRemovedNotifier;
        this.companyRemovedNotifier = companyRemovedNotifier;
        this.userLoginNotifier = userLoginNotifier;
        this.userLogoutNotifier = userLogoutNotifier;
        this.projectContentUpdatedNotifier = projectContentUpdatedNotifier;
        this.companyContentUpdatedNotifier = companyContentUpdatedNotifier;

        newProjectNotifier.Subscribe(this);
        newCompanyNotifier.Subscribe(this);
        projectRemovedNotifier.Subscribe(this);
        companyRemovedNotifier.Subscribe(this);
        userLoginNotifier.Subscribe(this);
        userLogoutNotifier.Subscribe(this);
        projectContentUpdatedNotifier.Subscribe(this);
        companyContentUpdatedNotifier.Subscribe(this);
    }

    public void Dispose()
    {
        newProjectNotifier.Unsubscribe(this);
        newCompanyNotifier.Unsubscribe(this);
        projectRemovedNotifier.Unsubscribe(this);
        companyRemovedNotifier.Unsubscribe(this);
        userLoginNotifier.Unsubscribe(this);
        userLogoutNotifier.Unsubscribe(this);
        projectContentUpdatedNotifier.Unsubscribe(this);
        companyContentUpdatedNotifier.Unsubscribe(this);
    }

    public void OnCompleted() { }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnNext(UserLoginEvent value)
    {
        logger.LogInformation($"User {value.userId} logged in at {DateTime.UtcNow}");
    }

    public void OnNext(NewCompanyEvent value)
    {
        logger.LogInformation($"User {value.userId} created company {value.CompanyId} at {DateTime.UtcNow}");
    }

    public void OnNext(NewProjectEvent value)
    {
        logger.LogInformation($"User {value.userId} created project {value.ProjectId} at {DateTime.UtcNow}");
    }

    public void OnNext(ProjectRemovedEvent value)
    {
        logger.LogInformation($"User {value.userId} removed project {value.ProjectId} at {DateTime.UtcNow}");
    }

    public void OnNext(CompanyRemovedEvent value)
    {
        logger.LogInformation($"User {value.userId} removed company {value.CompanyId} at {DateTime.UtcNow}");
    }

    public void OnNext(UserLogoutEvent value)
    {
        logger.LogInformation($"User {value.userId} logged out at {DateTime.UtcNow}");
    }

    public void OnNext(ProjectContentUpdatedEvent value)
    {
        logger.LogInformation($"User {value.userId} changed project Id:{value.Project.Id} Name: {value.Project.Name} at {DateTime.UtcNow}");
    }

    public void OnNext(CompanyContentUpdatedEvent value)
    {
        logger.LogInformation($"User {value.userId} changed company Id:{value.Company.Id} Name: {value.Company.Name} at {DateTime.UtcNow}");
    }
}
