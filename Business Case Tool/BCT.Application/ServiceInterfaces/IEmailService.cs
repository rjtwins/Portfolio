using BCT.Application.Dtos;

namespace BCT.Application.ServiceInterfaces;
public interface IEmailService
{
    public Task SendMail(Mail mail);
}
