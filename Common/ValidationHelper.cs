using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.AINewsletter.Configuration;

namespace Jellyfin.Plugin.AINewsletter.Common;

public static class ValidationHelper
{
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var mailAddress = new MailAddress(email);
            return mailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    public static ValidationResult ValidateConfiguration(PluginConfiguration config)
    {
        var result = new ValidationResult();

        // Validate AI configuration
        if (string.IsNullOrWhiteSpace(config.AIProvider))
        {
            result.AddError("AI Provider is required");
        }

        if (string.IsNullOrWhiteSpace(config.AIApiKey))
        {
            result.AddError("AI API Key is required");
        }

        if (string.IsNullOrWhiteSpace(config.AIModel))
        {
            result.AddError("AI Model is required");
        }

        if (!IsValidUrl(config.AIBaseUrl))
        {
            result.AddError("AI Base URL must be a valid URL");
        }

        // Validate SMTP configuration
        if (string.IsNullOrWhiteSpace(config.SmtpServer))
        {
            result.AddError("SMTP Server is required");
        }

        if (config.SmtpPort <= 0 || config.SmtpPort > 65535)
        {
            result.AddError("SMTP Port must be between 1 and 65535");
        }

        if (string.IsNullOrWhiteSpace(config.SmtpUsername))
        {
            result.AddError("SMTP Username is required");
        }

        if (string.IsNullOrWhiteSpace(config.SmtpPassword))
        {
            result.AddError("SMTP Password is required");
        }

        if (!IsValidEmail(config.SenderEmail))
        {
            result.AddError("Sender Email must be a valid email address");
        }

        // Validate recipients
        if (config.Recipients == null || config.Recipients.Length == 0)
        {
            result.AddError("At least one email recipient is required");
        }
        else
        {
            foreach (var recipient in config.Recipients)
            {
                if (!IsValidEmail(recipient))
                {
                    result.AddError($"Invalid email address: {recipient}");
                }
            }
        }

        // Validate scheduling configuration
        if (config.ScheduleIntervalHours <= 0)
        {
            result.AddError("Schedule Interval must be greater than 0");
        }

        if (config.DaysBackToScan <= 0)
        {
            result.AddError("Days Back to Scan must be greater than 0");
        }

        if (config.MaxItemsPerNewsletter <= 0)
        {
            result.AddError("Max Items per Newsletter must be greater than 0");
        }

        // Validate content types
        if (config.ContentTypes == null || config.ContentTypes.Length == 0)
        {
            result.AddError("At least one content type must be selected");
        }

        return result;
    }

    public static bool IsValidApiKey(string apiKey, string provider)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return false;

        return provider.ToLowerInvariant() switch
        {
            "openai" => apiKey.StartsWith("sk-") && apiKey.Length > 20,
            "anthropic" => apiKey.StartsWith("sk-ant-") && apiKey.Length > 20,
            "custom" => !string.IsNullOrWhiteSpace(apiKey),
            _ => false
        };
    }
}

public class ValidationResult
{
    private readonly System.Collections.Generic.List<string> _errors = new();

    public bool IsValid => _errors.Count == 0;
    public string[] Errors => _errors.ToArray();

    public void AddError(string error)
    {
        _errors.Add(error);
    }

    public string GetErrorsAsString()
    {
        return string.Join(Environment.NewLine, _errors);
    }
}