using UnityEngine;
using System.Net;
using System.Net.Mail;

public class SendMail : MonoBehaviour {

    public void Send(string user, string To, string link)
    {
        SmtpClient smtpServer = new SmtpClient("smtp.ipage.com");
        smtpServer.Port = 587;
        smtpServer.Credentials = new NetworkCredential("vrtuoztest@playo.com.br", "Test123456") as ICredentialsByHost;
        smtpServer.EnableSsl = false;
        MailAddress from = new MailAddress("vrtuoztest@playo.com.br", "Alex Chacon", System.Text.Encoding.UTF8);
        MailAddress to = new MailAddress(To);
        MailMessage mail = new MailMessage(from, to);
        mail.Subject = "App Authentication";
        mail.SubjectEncoding = System.Text.Encoding.UTF8;
        mail.Body = "Hey, " + user +
            "\rWelcome to my app, please click <a href='" + link + "'>HERE</a> to login." +
            "\rEnjoy!";
        mail.BodyEncoding = System.Text.Encoding.UTF8;
        mail.IsBodyHtml = true;
        string userState = "test message1";
        smtpServer.SendAsync(mail, userState);
        mail.Dispose();
    }
}