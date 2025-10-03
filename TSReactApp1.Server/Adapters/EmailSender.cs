using System.Diagnostics;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

using TSReactApp1.Server.Adapters;
using static System.Net.Mime.MediaTypeNames;
using TSReactApp1.Server.Domain.entities;
using MimeKit;

public class EmailSender : IEmailSender
{
    private readonly ILogger _logger;

    public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
                       ILogger<EmailSender> logger)
    {
        Options = optionsAccessor.Value;
        _logger = logger;
    }

    public AuthMessageSenderOptions Options { get; } //Set with Secret Manager.

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(Options.GoogleAppPassword))
        {
            throw new Exception("Null Password");
        }
        await Execute(Options.GoogleAppPassword, subject, message, toEmail);
    }

    public async Task Execute(string apiKey, string subject, string text, string toEmail)
    {
        var message = new MimeKit.MimeMessage();
        var from = "";
        // 宛先を追加  
        message.From.Add(new MimeKit.MailboxAddress("<宛先>", from));

        // 送信元を追加  
        message.To.Add(new MimeKit.MailboxAddress("<送信元>", toEmail));

        // 件名を設定
        message.Subject = subject;

        // Multipartクラスのインスタンスを生成
        var multipart = new MimeKit.Multipart("mixed")
            {
            // 本文を設定 
                new MimeKit.TextPart(MimeKit.Text.TextFormat.Plain)
                {
                    Text = text//"アカウント登録メールになります。身に覚えがない場合クリックしないでください。"
                },
            };

        message.Body = multipart;

        using (var client = new MailKit.Net.Smtp.SmtpClient())
        {
            try
            {
                // SMTPサーバに接続  
                await client.ConnectAsync("smtp.gmail.com", 587);
                Debug.WriteLine("接続完了");

                // SMTPサーバ認証  
                await client.AuthenticateAsync(from, "");

                // 送信  
                await client.SendAsync(message);
                Debug.WriteLine("送信完了");

                // 切断  
                await client.DisconnectAsync(true);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed send:{ex}");
            }
        }
    }
}
