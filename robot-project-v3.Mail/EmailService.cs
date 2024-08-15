using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Serilog;

namespace robot_project_v3.Mail;

public interface IEmailService
{
    Task SendEmail(string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly ILogger _logger;
    private readonly SmtpClient _smtpClient;
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger logger)
    {
        _logger = logger.ForContext<EmailService>();
        _smtpSettings = smtpSettings.Value;
        _smtpClient = new SmtpClient
        {
            Host = _smtpSettings.Host,
            Port = _smtpSettings.Port,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Password)
        };
    }

    public Task SendEmail(string subject, string body)
    {
        try
        {
            var fromAddress = new MailAddress(_smtpSettings.User);
            var toAddress = new MailAddress(_smtpSettings.DefaultEmail);

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                Priority = MailPriority.High
            };
            _logger.Information("Send mail : {@Mail}", message);
            _smtpClient.Send(message);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Can't send mail");
        }

        return Task.CompletedTask;
    }
}