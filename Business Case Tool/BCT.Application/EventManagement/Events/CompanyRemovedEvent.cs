using BCT.Domain.Entities;

namespace BCT.Application.EventManagement.Events;

public record CompanyRemovedEvent(int CompanyId, string userId);