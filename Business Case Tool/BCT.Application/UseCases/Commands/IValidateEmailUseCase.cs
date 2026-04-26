namespace BCT.Application.UseCases.Commands;

public interface ICheckIfEmailAvailableUseCase
{
	Task ExecuteAsync(string email);
}