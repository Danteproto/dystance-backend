using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;

        public EmailSender(EmailConfiguration emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public void SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);

            Send(emailMessage);
        }

        public async Task SendEmailAsync(Message message)
        {
            var mailMessage = CreateEmailMessage(message);

            await SendAsync(mailMessage);
        }

        private MailMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MailMessage();
            emailMessage.From = new MailAddress(_emailConfig.From);
            emailMessage.To.Add(message.To[0].ToString());
            emailMessage.Subject = message.Subject;

            //var bodyBuilder = new BodyBuilder { HtmlBody = string.Format("<h2 style='color:red;'>{0}</h2>", message.Content) };

            //if (message.Attachments != null && message.Attachments.Any())
            //{
            //    byte[] fileBytes;
            //    foreach (var attachment in message.Attachments)
            //    {
            //        using (var ms = new MemoryStream())
            //        {
            //            attachment.CopyTo(ms);
            //            fileBytes = ms.ToArray();
            //        }

            //        bodyBuilder.Attachments.Add(attachment.FileName, fileBytes);
            //    }
            //}

            //emailMessage.Body = bodyBuilder.ToMessageBody();

            emailMessage.Body = message.Content;
            emailMessage.IsBodyHtml = true;
            return emailMessage;
        }

        private void Send(MailMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    //    client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);
                    //    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    //    client.Authenticate(_emailConfig.UserName, _emailConfig.Password);

                    client.Send(mailMessage);
                }
                catch
                {
                    //log an error message or throw an exception, or both.
                    throw;
                }
                finally
                {
                    client.Dispose();
                }
            }
        }

        private async Task SendAsync(MailMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    //client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    //await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);
                    //client.AuthenticationMechanisms.Remove("XOAUTH2");
                    //var token = await ObtainAccessToken(_emailConfig.UserName);
                    //await client.AuthenticateAsync(token);

                    //await client.SendAsync(mailMessage);
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_emailConfig.UserName, _emailConfig.Password);
                    client.Host = "smtp.gmail.com";
                    client.Port = 587;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    await client.SendMailAsync(mailMessage);
                }
                catch
                {
                    //log an error message or throw an exception, or both.
                    throw;
                }
                finally
                {
                    //await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }



        //public async Task<SaslMechanismOAuth2> ObtainAccessToken(string gmailAccount)
        //{
        //    var clientSecrets = new ClientSecrets
        //    {
        //        ClientId = "862542023261-qme9ptagq1h0oll4ga664bf47v2iap9i.apps.googleusercontent.com",
        //        ClientSecret = "eBI14pWQamOM2XqBOQkj9ICP"
        //    };

        //    var codeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        //    {
        //        DataStore = new FileDataStore("CredentialCacheFolder", false),
        //        Scopes = new[] { "https://mail.google.com/" },
        //        ClientSecrets = clientSecrets
        //    });

        //    // Note: For a web app, you'll want to use AuthorizationCodeWebApp instead.
        //    var codeReceiver = new LocalServerCodeReceiver();
        //    var authCode = new AuthorizationCodeInstalledApp(codeFlow, codeReceiver);

        //    var credential = await authCode.AuthorizeAsync(gmailAccount, CancellationToken.None);

        //    if (credential.Token.IsExpired(SystemClock.Default))
        //        await credential.RefreshTokenAsync(CancellationToken.None);

        //    var oauth2 = new SaslMechanismOAuth2(credential.UserId, credential.Token.AccessToken);

        //    return oauth2;
        //}
    }
}
