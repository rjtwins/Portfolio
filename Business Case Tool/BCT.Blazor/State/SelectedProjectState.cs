using BCT.Application.EventManagement;
using BCT.Application.UseCases.Commands;
using BCT.Domain.Entities;

namespace BCT.Blazor.State;

internal class SelectedProjectState : Observable<Project> 
{
    private readonly IUpdateLastSelectedUseCase updateLastSelectedUseCase;
    private readonly CurrentUserState currentUserState;
    private bool SuspentNotifications = false;

    public SelectedProjectState(IUpdateLastSelectedUseCase updateLastSelectedUseCase, CurrentUserState currentUserState) 
    {
        this.updateLastSelectedUseCase = updateLastSelectedUseCase;
        this.currentUserState = currentUserState;
    }

    public void UpdateContentOnly(Project project)
    {
        lock (Value)
        {
            SuspentNotifications = true;
            Value = project;
            SuspentNotifications = false;
        }
    }

    protected override void Notify(Project value)
    {
        if (SuspentNotifications)
            return;

        base.Notify(value);

        var user = currentUserState.Value;
        if (user == null)
            return;

        updateLastSelectedUseCase.Execute(user.Id, value?.Id, value?.CompanyId);
    }
}
