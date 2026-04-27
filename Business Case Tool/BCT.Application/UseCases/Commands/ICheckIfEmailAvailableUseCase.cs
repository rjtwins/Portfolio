namespace BCT.Application.UseCases.Commands;

public interface ICheckIfEmailAvailableUseCase : IUseCase
{
	Task<bool> ExecuteAsync(string email);
}
