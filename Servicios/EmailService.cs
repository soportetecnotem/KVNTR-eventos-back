using EventosBack.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace EventosBack.Servicios
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            try
            {
                using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
                {
                    Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password),
                    EnableSsl = _smtpSettings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Timeout = 20000 // 20 segundos
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.From),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                throw new InvalidOperationException(
                    $"Error al enviar email. StatusCode: {ex.StatusCode}, Mensaje: {ex.Message}",
                    ex);
            }
        }
    }
}