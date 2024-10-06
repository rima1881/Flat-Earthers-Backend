using System.Net;
using System.Net.Mail;
using LandsatReflectance.Backend.Models;

namespace LandsatReflectance.Backend.Services.NotificationSender;

public class EmailNotificationSenderService : INotificationSenderService
{
    private readonly ILogger<EmailNotificationSenderService> m_logger;
    private readonly string m_fromAddress;
    private readonly string m_password;

    public EmailNotificationSenderService(ILogger<EmailNotificationSenderService> logger, KeysService keysService)
    {
        m_logger = logger;
        m_fromAddress = keysService.SmtpFromEmailAddress;
        m_password = keysService.SmtpPassword;
    }
    
    public void SendTargetNotification(User user, Target target)
    {
        var smtpClient = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(m_fromAddress, m_password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(m_fromAddress),
            Subject = $"Notification for target \"{target}\" (path, row: {target.Path}, {target.Row})",
            Body = $"Notification for target \"{target}\" (path, row: {target.Path}, {target.Row})"
        };
        
        mailMessage.To.Add(user.Email);
        smtpClient.Send(mailMessage);
        
        m_logger.LogInformation($"Email sent successfully to \"{user.Email}\" for target \"{target.Guid}\"");
    }

    /// <remarks>
    /// Param 'receiverInformation' should be an email.
    /// </remarks>
    public void SendGeneralNotification(string receiverInformation, string message, string? subject)
    {
        var smtpClient = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(m_fromAddress, m_password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(m_fromAddress),
            Subject = subject ?? "",
            Body = message 
        };
        
        mailMessage.To.Add(receiverInformation);
        smtpClient.Send(mailMessage);
        
        m_logger.LogInformation($"Email with subject \"{subject}\" sent successfully to \"{receiverInformation}\".");
    }
}