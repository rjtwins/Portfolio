using BCT.Application.EventManagement;
using BCT.Application.UseCases.Commands;

namespace BCT.Blazor.State;

internal class SelectedCompanyState : Observable<Domain.Entities.Company?>
{
    private readonly IUpdateLastSelectedUseCase updateLastSelectedUseCase;
    private readonly CurrentUserState currentUserState;

    public SelectedCompanyState(IUpdateLastSelectedUseCase updateLastSelectedUseCase, CurrentUserState currentUserState)
    {
        this.updateLastSelectedUseCase = updateLastSelectedUseCase;
        this.currentUserState = currentUserState;
    }

    protected override void Notify(Domain.Entities.Company? value)
    {
        base.Notify(value);

        var user = currentUserState.Value;
        if(user == null)
            return;

        updateLastSelectedUseCase.Execute(user.Id, null, value?.Id);
    }
}