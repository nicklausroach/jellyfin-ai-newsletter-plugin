using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Jellyfin.Plugin.AINewsletter.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AINewsletter.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private PluginConfiguration Configuration => Plugin.Instance?.Configuration ?? new PluginConfiguration();

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(Configuration.SmtpServer) &&
               !string.IsNullOrEmpty(Configuration.SmtpUsername) &&
               !string.IsNullOrEmpty(Configuration.SmtpPassword) &&
               !string.IsNullOrEmpty(Configuration.SenderEmail) &&
               Configuration.SmtpPort > 0;
    }

    public async Task<bool> SendEmailAsync(string recipient, string subject, string htmlBody)
    {
        if (!IsConfigured())
        {
            _logger.LogError("Email service is not properly configured");
            return false;
        }

        try
        {
            _logger.LogInformation("Sending email to {Recipient} with subject: {Subject}", recipient, subject);

            using var smtpClient = CreateSmtpClient();
            using var mailMessage = CreateMailMessage(recipient, subject, htmlBody);

            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation("Email sent successfully to {Recipient}", recipient);
            return true;
        }
        catch (SmtpAuthenticationException ex)
        {
            _logger.LogError(ex, "SMTP authentication failed when sending email to {Recipient}. Check username and password.", recipient);
            return false;
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error when sending email to {Recipient}: {Message}", recipient, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when sending email to {Recipient}", recipient);
            return false;
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        if (!IsConfigured())
        {
            _logger.LogWarning("Cannot test connection - email service is not properly configured");
            return false;
        }

        try
        {
            _logger.LogInformation("Testing SMTP connection to {Server}:{Port}", Configuration.SmtpServer, Configuration.SmtpPort);

            using var smtpClient = CreateSmtpClient();
            
            // Test connection by connecting and authenticating
            await Task.Run(() =>
            {
                smtpClient.Send(CreateTestMessage());
            });

            _logger.LogInformation("SMTP connection test successful");
            return true;
        }
        catch (SmtpAuthenticationException ex)
        {
            _logger.LogError(ex, "SMTP authentication failed during connection test. Check username and password.");
            return false;
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP connection test failed: {Message}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during SMTP connection test");
            return false;
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        var smtpClient = new SmtpClient(Configuration.SmtpServer, Configuration.SmtpPort)
        {
            EnableSsl = Configuration.SmtpUseSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(Configuration.SmtpUsername, Configuration.SmtpPassword),
            Timeout = 30000, // 30 seconds timeout
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        return smtpClient;
    }

    private MailMessage CreateMailMessage(string recipient, string subject, string htmlBody)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(Configuration.SenderEmail, Configuration.SenderName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
            Priority = MailPriority.Normal
        };

        mailMessage.To.Add(new MailAddress(recipient));

        // Set proper headers for better deliverability
        mailMessage.Headers.Add("X-Mailer", "Jellyfin AI Newsletter Plugin");
        mailMessage.Headers.Add("X-Priority", "3");
        mailMessage.Headers.Add("X-MSMail-Priority", "Normal");

        return mailMessage;
    }

    private MailMessage CreateTestMessage()
    {
        var testHtml = @"
        <!DOCTYPE html>
        <html>
        <head>
            <title>SMTP Test</title>
        </head>
        <body>
            <h2>SMTP Connection Test</h2>
            <p>This is a test email to verify SMTP configuration.</p>
            <p>If you receive this email, your SMTP settings are working correctly.</p>
            <hr>
            <p><small>Generated by Jellyfin AI Newsletter Plugin</small></p>
        </body>
        </html>";

        return CreateMailMessage(Configuration.SenderEmail, "SMTP Test - Jellyfin AI Newsletter", testHtml);
    }
}