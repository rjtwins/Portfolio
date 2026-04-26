namespace BCT.Application.UseCases.Queries;
public class GetUserHasAccessToCompany
{
    public GetUserHasAccessToCompany()
    {

    }

    public async Task<bool> ExecuteAsync(User user, Company company)
    {
        return true;
    }
}
