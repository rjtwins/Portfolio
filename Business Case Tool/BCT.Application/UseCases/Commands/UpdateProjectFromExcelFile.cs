namespace BCT.Application.UseCases.Commands;

public class UpdateProjectFromExcelFile : IUpdateProjectFromExcelFile
{
	private readonly IExcelConverter excelConverter;

	public UpdateProjectFromExcelFile(IExcelConverter excelConverter)
	{
		this.excelConverter = excelConverter;
	}
	
	public async Task ExecuteAsync(Project project, string excel)
	{
		//var result = await excelConverter.ConvertProjectFromExcelAsync(excel);
	}
}
