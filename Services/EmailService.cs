using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
public class EmailService
{
private readonly SmtpSettings _smtp;
private readonly AppSettings _appSettings;

public EmailService(IOptions<SmtpSettings> smtp, IOptions<AppSettings> appSettings)
{
    _smtp = smtp.Value;
    _appSettings = appSettings.Value;
}
    public async Task<bool>SendVerificationEmail(string email, string username, string token)
    {
        try
        {
        var verificationLink = $"{_appSettings.BaseUrl}/api/auth/verify?token={token}";

        var smtpClient = new SmtpClient(_smtp.Host)
        {
            Port = _smtp.Port,
            Credentials = new NetworkCredential(_smtp.User, _smtp.Pass),
            EnableSsl = true
        };

       string body = $@"
        <p>Hello {username},</p>
        <p>Please verify your account by clicking the link below:</p>
        <p><a href='{verificationLink}'>Verify your account</a></p>
        <p>Or copy and paste this URL into your browser: {verificationLink}</p>
        <p>If you did not request this, please ignore this email.</p>
        ";

        var message = new MailMessage
        {
            From = new MailAddress(_smtp.User),
            Subject = "Verify your account",
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(email);
        await smtpClient.SendMailAsync(message);
        return true;
        }
       catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
            return false;
        }
       
    }
}
