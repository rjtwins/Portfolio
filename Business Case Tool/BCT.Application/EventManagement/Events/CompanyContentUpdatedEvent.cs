namespace BCT.Application.EventManagement.Events;

public record CompanyContentUpdatedEvent(Company Company, string userId);