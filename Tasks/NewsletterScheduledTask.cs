using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.AINewsletter.Configuration;
using Jellyfin.Plugin.AINewsletter.Services;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AINewsletter.Tasks;

public class NewsletterScheduledTask : IScheduledTask
{
    private readonly INewsletterService _newsletterService;
    private readonly ILogger<NewsletterScheduledTask> _logger;
    private PluginConfiguration Configuration => Plugin.Instance?.Configuration ?? new PluginConfiguration();

    public NewsletterScheduledTask(INewsletterService newsletterService, ILogger<NewsletterScheduledTask> logger)
    {
        _newsletterService = newsletterService;
        _logger = logger;
    }

    public string Name => "Generate AI Newsletter";

    public string Description => "Automatically generates and sends AI-powered email newsletters featuring recently added media content";

    public string Category => "AI Newsletter";

    public string Key => "AINewsletterGeneration";

    public bool IsHidden => false;

    public bool IsEnabled => Configuration.EnableScheduledTask;

    public bool IsLogged => true;

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting scheduled newsletter generation task");
            progress.Report(0);

            // Check if the service is properly configured
            if (!IsServiceConfigured())
            {
                _logger.LogWarning("Newsletter service is not properly configured. Skipping newsletter generation.");
                progress.Report(100);
                return;
            }

            progress.Report(10);

            // Check if newsletter generation is enabled
            if (!Configuration.EnableScheduledTask)
            {
                _logger.LogInformation("Scheduled newsletter generation is disabled in configuration");
                progress.Report(100);
                return;
            }

            progress.Report(20);

            // Generate and send newsletter
            _logger.LogInformation("Generating and sending newsletter...");
            var success = await _newsletterService.GenerateAndSendNewsletterAsync();

            progress.Report(90);

            if (success)
            {
                _logger.LogInformation("Newsletter generation and sending completed successfully");
            }
            else
            {
                _logger.LogWarning("Newsletter generation completed but may have had some failures");
            }

            progress.Report(100);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Newsletter generation task was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during scheduled newsletter generation");
            throw;
        }
    }

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        // Default to daily execution at 9 AM
        return new[]
        {
            new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerDaily,
                TimeOfDayTicks = TimeSpan.FromHours(9).Ticks,
                MaxRuntimeTicks = TimeSpan.FromMinutes(30).Ticks
            }
        };
    }

    private bool IsServiceConfigured()
    {
        // Check if basic configuration is in place
        var hasRecipients = Configuration.Recipients?.Length > 0;
        var hasSmtpConfig = !string.IsNullOrEmpty(Configuration.SmtpServer) &&
                           !string.IsNullOrEmpty(Configuration.SmtpUsername) &&
                           !string.IsNullOrEmpty(Configuration.SenderEmail);
        var hasAIConfig = !string.IsNullOrEmpty(Configuration.AIApiKey) &&
                         !string.IsNullOrEmpty(Configuration.AIProvider);

        if (!hasRecipients)
        {
            _logger.LogWarning("No email recipients configured");
        }

        if (!hasSmtpConfig)
        {
            _logger.LogWarning("SMTP configuration is incomplete");
        }

        if (!hasAIConfig)
        {
            _logger.LogWarning("AI service configuration is incomplete");
        }

        return hasRecipients && hasSmtpConfig && hasAIConfig;
    }
}