namespace Jellyfin.Plugin.AINewsletter.Common;

public static class Constants
{
    public const string PluginName = "AI Newsletter";
    public const string PluginGuid = "8B0B7F7A-3F8C-4F5D-9C2A-1E5F8B7A3C4D";
    public const string PluginVersion = "1.0.0.2";
    
    public static class LoggingEvents
    {
        public const int Newsletter = 1000;
        public const int AIService = 1100;
        public const int EmailService = 1200;
        public const int MediaAnalyzer = 1300;
        public const int TemplateService = 1400;
        public const int Configuration = 1500;
        public const int ScheduledTask = 1600;
    }
    
    public static class ErrorMessages
    {
        public const string AIServiceNotConfigured = "AI service is not properly configured. Please check your API key and provider settings.";
        public const string EmailServiceNotConfigured = "Email service is not properly configured. Please check your SMTP settings.";
        public const string NoRecipientsConfigured = "No email recipients have been configured.";
        public const string NoMediaItemsFound = "No new media items found in the specified time period.";
        public const string NewsletterGenerationFailed = "Failed to generate newsletter content.";
        public const string EmailSendFailed = "Failed to send email to one or more recipients.";
    }
    
    public static class DefaultValues
    {
        public const string DefaultTone = "friendly";
        public const string DefaultAIProvider = "OpenAI";
        public const string DefaultAIModel = "gpt-4o-mini";
        public const string DefaultAIBaseUrl = "https://api.openai.com/v1";
        public const string DefaultSenderName = "Jellyfin AI Newsletter";
        public const string DefaultEmailSubject = "🎬 Your Weekly Jellyfin Update - {ItemCount} New Items";
        public const string DefaultPosterHosting = "JellyfinAPI";
        public const int DefaultScheduleIntervalHours = 24;
        public const int DefaultDaysBackToScan = 7;
        public const int DefaultMaxItemsPerNewsletter = 10;
        public const int DefaultSmtpPort = 587;
        public static readonly string[] DefaultContentTypes = { "Movie", "Series", "MusicAlbum" };
    }
}