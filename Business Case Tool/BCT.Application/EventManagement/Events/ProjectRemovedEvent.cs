using BCT.Domain.Entities;

namespace BCT.Application.EventManagement.Events;

public record ProjectRemovedEvent(int ProjectId, string userId);