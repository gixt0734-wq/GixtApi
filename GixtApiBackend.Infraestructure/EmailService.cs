using System.Net;
using System.Net.Mail;

public class EmailService
{
    public async Task SendEmailAsync(string email, string body)
    {
        var fromAddress = new MailAddress("gixt0734@gmail.com", "GIXT");
        var toAddress = new MailAddress(email);

        const string fromPassword = "pqhkkpqgsrgartyl";

        string subject = "Código de verificación";

        

        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
        };

        using var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        await smtp.SendMailAsync(message);
    }

}
