namespace BCT.Application.EventManagement.Events;

public record UserAuthenticatedEvent(string AuthId, bool IsAuthenticated);
