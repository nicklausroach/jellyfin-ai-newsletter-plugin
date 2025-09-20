using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.AINewsletter.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public string AIProvider { get; set; } = "OpenAI";
    
    public string AIApiKey { get; set; } = string.Empty;
    
    public string AIModel { get; set; } = "gpt-4o-mini";
    
    public string AIBaseUrl { get; set; } = "https://api.openai.com/v1";
    
    public string NewsletterTone { get; set; } = "friendly";
    
    public bool EnablePersonalization { get; set; } = true;
    
    public string SmtpServer { get; set; } = string.Empty;
    
    public int SmtpPort { get; set; } = 587;
    
    public string SmtpUsername { get; set; } = string.Empty;
    
    public string SmtpPassword { get; set; } = string.Empty;
    
    public bool SmtpUseSsl { get; set; } = true;
    
    public string SenderEmail { get; set; } = string.Empty;
    
    public string SenderName { get; set; } = "Jellyfin AI Newsletter";
    
    public string[] Recipients { get; set; } = Array.Empty<string>();
    
    public int ScheduleIntervalHours { get; set; } = 24;
    
    public string[] IncludedLibraries { get; set; } = Array.Empty<string>();
    
    public string[] ContentTypes { get; set; } = { "Movie", "Series", "MusicAlbum" };
    
    public int DaysBackToScan { get; set; } = 7;
    
    public int MaxItemsPerNewsletter { get; set; } = 10;
    
    public bool IncludePosters { get; set; } = true;
    
    public string PosterHostingType { get; set; } = "JellyfinAPI";
    
    public string ImgurClientId { get; set; } = string.Empty;
    
    public string EmailSubjectTemplate { get; set; } = "🎬 Your Weekly Jellyfin Update - {ItemCount} New Items";
    
    public bool EnableScheduledTask { get; set; } = true;
    
    public string CustomPromptAdditions { get; set; } = string.Empty;
}