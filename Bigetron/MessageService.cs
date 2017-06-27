namespace Bigetron
{
    using System;
    using System.Threading.Tasks;
    using MailKit.Net.Smtp;
    using Microsoft.Extensions.Configuration;
    using MimeKit;

    public class MessageService
    {
        #region Private Members
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public MessageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        public async Task SendEmailAsync(
            string to,
            string from,
            string subject,
            string plainTextMessage,
            string htmlMessage,
            string replyTo = null)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("no to address provided");

            if (string.IsNullOrWhiteSpace(from))
                throw new ArgumentException("no from address provided");

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("no subject provided");

            var hasPlainText = !string.IsNullOrWhiteSpace(plainTextMessage);
            var hasHtml = !string.IsNullOrWhiteSpace(htmlMessage);
            if (!hasPlainText && !hasHtml)
                throw new ArgumentException("no message provided");

            var m = new MimeMessage();

            m.From.Add(new MailboxAddress("", from));
            if (!string.IsNullOrWhiteSpace(replyTo))
                m.ReplyTo.Add(new MailboxAddress("", replyTo));
            m.To.Add(new MailboxAddress("", to));
            m.Subject = subject;

            //m.Importance = MessageImportance.Normal;
            //Header h = new Header(HeaderId.Precedence, "Bulk");
            //m.Headers.Add()

            var bodyBuilder = new BodyBuilder();
            if (hasPlainText)
                bodyBuilder.TextBody = plainTextMessage;

            if (hasHtml)
                bodyBuilder.HtmlBody = htmlMessage;

            m.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_configuration["SMTP:Server"], int.Parse(_configuration["SMTP:Port"]),
                        bool.Parse(_configuration["SMTP:UseSsl"]))
                    .ConfigureAwait(false);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                // Note: only needed if the SMTP server requires authentication
                if (bool.Parse(_configuration["SMTP:RequiresAuthentication"]))
                    await client.AuthenticateAsync(_configuration["SMTP:User"], _configuration["SMTP:Password"])
                        .ConfigureAwait(false);

                await client.SendAsync(m).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
    }
}
