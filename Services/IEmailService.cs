using System.Threading.Tasks;

namespace Gestion_Universitaire.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task EnvoyerCode(string email, string code);
    }
}
