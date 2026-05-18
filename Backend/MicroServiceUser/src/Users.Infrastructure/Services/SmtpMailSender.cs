using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Taller_Mecanico_Users.Infrastructure.Security ;
using Taller_Mecanico_Users.Domain.Interfaces;

namespace Taller_Mecanico_Users.Infrastructure.Services
{
    public class SmtpMailSender : IMailSender
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<SmtpMailSender> _logger;

        public SmtpMailSender(SmtpSettings settings, ILogger<SmtpMailSender> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (!_settings.Enabled)
            {
                _logger.LogWarning("SMTP is disabled in configuration. Email not sent. To={To} Subject={Subject}", to, subject);
                Console.WriteLine($"[SMTP disabled] To: {to} Subject: {subject}\n{body}");
                return;
            }

            using var message = new MailMessage();
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = body ?? string.Empty;
            message.IsBodyHtml = true;
            message.From = new MailAddress(_settings.From);

            using var client = new SmtpClient(_settings.Host, _settings.Port) {
                EnableSsl = _settings.EnableSsl,
                Timeout = _settings.TimeoutMs,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
            }

            try
            {
                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }
    }
}

