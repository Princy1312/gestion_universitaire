using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Gestion_Universitaire.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");

                using var smtpClient = new SmtpClient(smtpSettings["SmtpServer"])
                {
                    Port = int.Parse(smtpSettings["SmtpPort"] ?? "587"),
                    Credentials = new NetworkCredential(
                        smtpSettings["Username"],
                        smtpSettings["Password"]
                    ),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true"),
                    UseDefaultCredentials = bool.Parse(smtpSettings["UseDefaultCredentials"] ?? "false"),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 10000 // 10 secondes de timeout
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        smtpSettings["SenderEmail"] ?? throw new ArgumentNullException("SenderEmail is not configured"),
                        smtpSettings["SenderName"]
                    ),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    Priority = MailPriority.Normal
                };

                mailMessage.To.Add(toEmail);

                // Ajout d'un gestionnaire d'événements pour le débogage
                smtpClient.SendCompleted += (s, e) => {
                    if (e.Error != null)
                    {
                        Console.WriteLine($"Erreur d'envoi d'email: {e.Error.Message}");
                    }
                    else if (e.Cancelled)
                    {
                        Console.WriteLine("Envoi d'email annulé");
                    }
                    else
                    {
                        Console.WriteLine("Email envoyé avec succès");
                    }
                };

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi de l'email: {ex.Message}");
                throw; // Relancer l'exception pour que le contrôleur puisse la gérer
            }
        }

        public async Task EnvoyerCode(string email, string code)
        {
            var subject = "Votre code de vérification en deux étapes";
            var body = $@"
            <h2>Votre code de vérification</h2>
            <p>Utilisez le code suivant pour vérifier votre inscription :</p>
            <h1 style='color: #2563eb; font-size: 2em;'>{code}</h1>
            <p>Ce code expirera dans 5 minutes.</p>
            <p>Si vous n'avez pas demandé ce code, vous pouvez ignorer cet email.</p>";

            await SendEmailAsync(email, subject, body);
        }
    }
}
