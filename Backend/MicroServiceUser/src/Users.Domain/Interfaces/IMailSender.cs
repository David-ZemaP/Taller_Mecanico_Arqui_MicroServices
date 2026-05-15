namespace Taller_Mecanico_Users.Domain.Interfaces
{
    public interface IMailSender
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}