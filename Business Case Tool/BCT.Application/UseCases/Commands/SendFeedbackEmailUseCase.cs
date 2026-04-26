using BCT.Application.Dtos;
using BCT.Application.Services;
using BCT.Application.UseCases.Queries;
using Microsoft.Extensions.Configuration;

namespace BCT.Application.UseCases.Commands;
public class SendFeedbackEmailUseCase : ISendFeedbackEmailUseCase
{
    private readonly IEmailService emailService;
    private readonly IConfiguration configuration;
    private readonly IGetUserUseCase getUserUseCase;

    public SendFeedbackEmailUseCase(
        IEmailService emailService, 
        IConfiguration configuration, 
        IGetUserUseCase getUserUseCase)
    {
        this.emailService = emailService;
        this.configuration = configuration;
        this.getUserUseCase = getUserUseCase;
    }

    public async Task ExecuteAsync(string userAuthId, string message, string instanceIdentifier)
    {
        var user = await getUserUseCase.ExecuteAsync(userAuthId);
        var mailAdres = configuration["FeedbackEmail:Email"];
        var subject = $"User {user.Email} feedback {instanceIdentifier}";

        var mail = new Mail(mailAdres, mailAdres, "", "", subject, message);

        await emailService.SendMail(mail);
    }
}
