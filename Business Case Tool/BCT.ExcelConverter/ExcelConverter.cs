using BCT.Application.ServiceInterfaces;
using BCT.Application.UseCases.Queries;
using BCT.Domain.Entities;
﻿using FluentExcel;
using NPOI.SS.UserModel;

namespace BCT.ExcelConverter;

public class ExcelConverter : IExcelConverter
{
	public static Semaphore semaphore = new Semaphore(1, 1);
	//private readonly IGetProjectOverTimeValuesUseCase getProjectOverTimeValuesUseCase;

	//public ExcelConverter(IGetProjectOverTimeValuesUseCase getProjectOverTimeValuesUseCase)
	//{
	//	this.getProjectOverTimeValuesUseCase = getProjectOverTimeValuesUseCase;
	//}

//	public async Task<(Project, List<OverTimeValue>)> ConvertProjectFromExcelAsync(string excel)
//	{
//		semaphore.WaitOne();
//		var setting = GetExcelSetting();	
		
//		try
//		{
//			var bytes = Convert.FromBase64String(excel);
//			string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");
//			await File.WriteAllBytesAsync(tempFilePath, bytes);
			
//			var projectDto = Excel.Load<ProjectDto>(tempFilePath, setting, 1, 0).ToList().Single();
//			var ovts = Excel.Load<OverTimeValueDto>(tempFilePath, setting, 1, 1).ToList();
			
//#if !DEBUG
//			File.Delete(tempFilePath);
//#endif
//			return (projectDto.EnrichProject(new Project()), ovts.Select(x => x.Enrich(new OverTimeValue())).ToList());
//		}
//		catch (Exception)
//		{
//			throw;
//		}finally
//		{
//			semaphore.Release();
//		}
//	}

//	public async Task<string> ConvertProjectToExcelAsync(Project project)
//	{
//		semaphore.WaitOne();
//		Excel.Setting = GetExcelSetting();
		
//		try
//		{
//			var ovts = await getProjectOverTimeValuesUseCase.ExecuteAsync(project);
			
//			var ovtDtos = ovts
//				.OrderBy(x => x.YearNr)
//				.ThenBy(x => x.QuarterNr)
//				.Select(x => x.ToDto())
//				.ToList();
				
//			var projectDto = project.ToDto();
			
//			string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");
			
//			new List<ProjectDto> { projectDto }.ToExcel(tempFilePath, "Project", 1, true);
//			ovtDtos.ToList().ToExcel(tempFilePath, "Data", ovts.Count, true);
			
//			var bytes = await File.ReadAllBytesAsync(tempFilePath);
//			var base64 = Convert.ToBase64String(bytes);

//#if !DEBUG
//			File.Delete(tempFilePath);
//#endif
//			return base64;
//		}
//		catch (Exception)
//		{
//			throw;
//		}
//		finally
//		{
//			semaphore.Release();
//		}
//	}
	
	private ExcelSetting GetExcelSetting()
	{
		var setting = new ExcelSetting();
		setting.UseXlsx = true;
		setting.TitleCellStyleApplier = (ICellStyle style, IFont font) => 
		{
			style.ShrinkToFit = true;
		};
		
		var projectSetting = setting.For<ProjectDto>();
		projectSetting.Property(x => x.EndTime)
			.HasDataFormatter("yyyy-MM")
			.HasAutoIndex();
		projectSetting.Property(x => x.StartTime)
			.HasDataFormatter("yyyy-MM")
			.HasAutoIndex();
		
		projectSetting.AdjustAutoIndex();
		
		return setting;
	}
}

public class ProjectDto
{
	public string Name {get; set;}
	public DateTime StartTime {get; set;}
	public DateTime EndTime {get; set;}
	public ProjectDto()
	{
		
	}
}

public class OverTimeValueDto
{
	public string Quarter { get; set; }
	public double Cost { get; set; }
	public double Cost2 { get; set; }
	public double Cost3 { get; set; }
	public double Cost4 { get; set; }
	public double Revenue { get; set; }
	public double Revenue2 { get; set; }
	public double Revenue3 { get; set; }
	public double Revenue4 { get; set; }
	public OverTimeValueDto()
	{
		
	}
}

public static class DtoExtensions
{
	public static Project EnrichProject(this ProjectDto dto, Project project)
	{
		project.Name = dto.Name;
		//project.StartTime = dto.StartTime;
		//project.EndTime = dto.EndTime;

		return project;
	}
	
	public static ProjectDto ToDto(this Project project)
	{
		return new ProjectDto()
		{
			Name = project.Name, 
			//StartTime = project.StartTime, 
			//EndTime = project.EndTime
		};
	}
	
	//public static OverTimeValue Enrich(this OverTimeValueDto dto, OverTimeValue ovt)
	//{
	//	//ovt.Cost = dto.Cost;
	//	//ovt.Cost2 = dto.Cost2;
	//	//ovt.Cost3 = dto.Cost3;
	//	//ovt.Cost4 = dto.Cost4;
	//	//ovt.Revenue = dto.Revenue;
	//	//ovt.Revenue2 = dto.Revenue2;
	//	//ovt.Revenue3 = dto.Revenue3;
	//	//ovt.Revenue4 = dto.Revenue4;
	//	//ovt.Quarter = dto.Quarter;
	//	return ovt;
	//}
	
	//public static OverTimeValueDto ToDto(this OverTimeValue overTimeValue)
	//{
	//	return new OverTimeValueDto
	//	{
	//		//Quarter = overTimeValue.Quarter,
	//		//Cost = overTimeValue.Cost,
	//		//Cost2 = overTimeValue.Cost2,
	//		//Cost3 = overTimeValue.Cost3,
	//		//Cost4 = overTimeValue.Cost4,
	//		//Revenue = overTimeValue.Revenue,
	//		//Revenue2 = overTimeValue.Revenue2,
	//		//Revenue3 = overTimeValue.Revenue3,
	//		//Revenue4 = overTimeValue.Revenue4
	//	};
	//}
}