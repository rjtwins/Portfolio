
namespace BCT.Application.UseCases.Queries;

public interface IGetSensitivityAnalysisResultUseCase : IUseCase
{
    Task<List<(string Property, double PosEffect, double NegEffect)>> ExecuteAsync(int projectId, string property);
}