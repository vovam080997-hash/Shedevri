using System.Net;
using System.Net.Mail;

namespace CinemaGo.Services;


public class EmailService
{
    private readonly string smtpHost;
    private readonly int smtpPort;
    private readonly string smtpUser;
    private readonly string smtpPassword;
    private readonly bool enabled;

    public EmailService(string smtpHost = "smtp.example.com", int smtpPort = 587,
        string smtpUser = "cinemago@example.com", string smtpPassword = "changeme", bool enabled = false)
    {
        this.smtpHost = smtpHost;
        this.smtpPort = smtpPort;
        this.smtpUser = smtpUser;
        this.smtpPassword = smtpPassword;

        this.enabled = enabled;
    }

    public void SendBookingConfirmation(string toEmail, string subject, string body)
    {
        if (!enabled)
        {
            Logger.Log($"EMAIL (simulated, SMTP disabled) -> {toEmail}: {subject}");
            Console.WriteLine("(Confirmation email sent — simulation mode, see log.txt)");
            return;
        }

        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = true
            };

            using var message = new MailMessage(smtpUser, toEmail, subject, body);
            client.Send(message);

            Logger.Log($"EMAIL sent successfully -> {toEmail}: {subject}");
        }
        catch (Exception ex)
        {

            Logger.Log($"EMAIL error -> {toEmail}: {ex.Message}");
            Console.WriteLine("Warning: failed to send the confirmation email (see log.txt).");
        }
    }
}
