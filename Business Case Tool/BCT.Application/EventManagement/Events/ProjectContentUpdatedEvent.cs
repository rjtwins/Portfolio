using BCT.Domain.Entities;

namespace BCT.Application.EventManagement.Events;
public record ProjectContentUpdatedEvent(Project Project, string userId);
