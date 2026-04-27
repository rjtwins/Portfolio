namespace BCT.Application.UseCases.Commands;

public class SaveProjectToExcelUseCase : ISaveProjectToExcelUseCase
{
    private readonly IExcelConverter excelConverter;

    public SaveProjectToExcelUseCase(IExcelConverter excelConverter)
	{
        this.excelConverter = excelConverter;
    }
	
	public async Task<string> ExecuteAsync(Project project)
	{
        return string.Empty;
		//return await excelConverter.ConvertProjectToExcelAsync(project);
	}
}
