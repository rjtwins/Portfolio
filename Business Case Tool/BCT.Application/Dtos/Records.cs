namespace BCT.Application.Dtos;
public record Mail(string From, string To, string CC, string BCC, string Subject, string ContentBody);