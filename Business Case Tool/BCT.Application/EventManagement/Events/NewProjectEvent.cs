using BCT.Domain.Entities;
namespace BCT.Application.EventManagement.Events;
public record NewProjectEvent(int ProjectId, string userId);
