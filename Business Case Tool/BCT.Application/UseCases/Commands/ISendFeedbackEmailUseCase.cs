using BCT.Application.Dtos;

namespace BCT.Application.UseCases.Commands;
public interface ISendFeedbackEmailUseCase : IUseCase
{
    Task ExecuteAsync(string userAuthId, string message, string instanceIdentifier);
}