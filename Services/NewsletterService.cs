using System;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.AINewsletter.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AINewsletter.Services;

public class NewsletterService : INewsletterService
{
    private readonly IMediaAnalyzer _mediaAnalyzer;
    private readonly IAIService _aiService;
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly ILogger<NewsletterService> _logger;
    
    private PluginConfiguration Configuration => Plugin.Instance?.Configuration ?? new PluginConfiguration();

    public NewsletterService(
        IMediaAnalyzer mediaAnalyzer,
        IAIService aiService,
        IEmailService emailService,
        ITemplateService templateService,
        ILogger<NewsletterService> logger)
    {
        _mediaAnalyzer = mediaAnalyzer;
        _aiService = aiService;
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<bool> GenerateAndSendNewsletterAsync()
    {
        try
        {
            _logger.LogInformation("Starting newsletter generation and send process");

            // Check if we have recipients
            if (!Configuration.Recipients.Any())
            {
                _logger.LogWarning("No recipients configured, skipping newsletter generation");
                return false;
            }

            // Get recently added media items
            var sinceDate = DateTime.UtcNow.AddDays(-Configuration.DaysBackToScan);
            var mediaItems = await _mediaAnalyzer.GetRecentlyAddedItemsAsync(
                sinceDate,
                Configuration.IncludedLibraries,
                Configuration.ContentTypes,
                Configuration.MaxItemsPerNewsletter
            );

            if (!mediaItems.Any())
            {
                _logger.LogInformation("No new media items found since {SinceDate}, skipping newsletter", sinceDate);
                return false;
            }

            _logger.LogInformation("Found {ItemCount} new media items to include in newsletter", mediaItems.Count);

            // Generate AI content for the newsletter
            var newsletterContent = await _aiService.GenerateNewsletterAsync(
                mediaItems,
                Configuration.NewsletterTone,
                Configuration.EnablePersonalization,
                Configuration.CustomPromptAdditions
            );

            // Generate HTML email
            var htmlContent = await _templateService.GenerateEmailHtmlAsync(newsletterContent);

            // Generate email subject
            var subject = Configuration.EmailSubjectTemplate.Replace("{ItemCount}", mediaItems.Count.ToString());

            // Send to all recipients
            var sendTasks = Configuration.Recipients.Select(recipient =>
                _emailService.SendEmailAsync(recipient, subject, htmlContent)
            );

            var results = await Task.WhenAll(sendTasks);
            var successCount = results.Count(r => r);
            var failureCount = results.Length - successCount;

            _logger.LogInformation("Newsletter sent successfully to {SuccessCount} recipients, {FailureCount} failures", 
                successCount, failureCount);

            return successCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate and send newsletter");
            return false;
        }
    }

    public async Task<string> GenerateNewsletterHtmlAsync()
    {
        try
        {
            var sinceDate = DateTime.UtcNow.AddDays(-Configuration.DaysBackToScan);
            var mediaItems = await _mediaAnalyzer.GetRecentlyAddedItemsAsync(
                sinceDate,
                Configuration.IncludedLibraries,
                Configuration.ContentTypes,
                Configuration.MaxItemsPerNewsletter
            );

            if (!mediaItems.Any())
            {
                return "<p>No new media items found in the specified time period.</p>";
            }

            var newsletterContent = await _aiService.GenerateNewsletterAsync(
                mediaItems,
                Configuration.NewsletterTone,
                Configuration.EnablePersonalization,
                Configuration.CustomPromptAdditions
            );

            return await _templateService.GenerateEmailHtmlAsync(newsletterContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate newsletter HTML");
            return $"<p>Error generating newsletter: {ex.Message}</p>";
        }
    }

    public async Task<bool> SendTestEmailAsync(string testRecipient)
    {
        try
        {
            _logger.LogInformation("Sending test email to {Recipient}", testRecipient);

            var testHtml = await GenerateTestEmailHtmlAsync();
            var subject = "Test Email - Jellyfin AI Newsletter";

            return await _emailService.SendEmailAsync(testRecipient, subject, testHtml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test email to {Recipient}", testRecipient);
            return false;
        }
    }

    private async Task<string> GenerateTestEmailHtmlAsync()
    {
        // Create sample media items for testing
        var testMediaItems = new[]
        {
            new Models.MediaItemInfo
            {
                Id = "test1",
                Title = "The Matrix",
                Type = "Movie",
                Year = 1999,
                Overview = "A computer hacker learns from mysterious rebels about the true nature of his reality.",
                Genres = new[] { "Action", "Sci-Fi" },
                Director = "The Wachowskis",
                CommunityRating = 8.7f,
                DateAdded = DateTime.UtcNow.AddDays(-1)
            },
            new Models.MediaItemInfo
            {
                Id = "test2",
                Title = "Stranger Things",
                Type = "Series",
                Year = 2016,
                Overview = "When a young boy disappears, his mother, a police chief and his friends must confront terrifying supernatural forces.",
                Genres = new[] { "Drama", "Fantasy", "Horror" },
                CommunityRating = 8.7f,
                DateAdded = DateTime.UtcNow.AddDays(-2)
            }
        }.ToList();

        var testContent = new Models.NewsletterContent
        {
            Title = "Test Newsletter - Your Weekly Jellyfin Update",
            Introduction = "This is a test email to verify your newsletter configuration is working correctly.",
            Sections = new[]
            {
                new Models.NewsletterSection
                {
                    SectionTitle = "🎬 New Movies",
                    Description = "Fresh movies added to your collection",
                    Items = testMediaItems.Where(i => i.Type == "Movie").ToList()
                },
                new Models.NewsletterSection
                {
                    SectionTitle = "📺 TV Shows",
                    Description = "New series to binge-watch",
                    Items = testMediaItems.Where(i => i.Type == "Series").ToList()
                }
            }.ToList(),
            Conclusion = "This was a test email. If you received this, your newsletter setup is working perfectly!"
        };

        return await _templateService.GenerateEmailHtmlAsync(testContent);
    }
}