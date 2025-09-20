using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.Plugin.AINewsletter.Models;

namespace Jellyfin.Plugin.AINewsletter.Services;

public interface IAIService
{
    Task<NewsletterContent> GenerateNewsletterAsync(List<MediaItemInfo> mediaItems, string tone, bool enablePersonalization, string? customPrompt = null);
    
    Task<string> GenerateItemDescriptionAsync(MediaItemInfo item, string tone);
    
    Task<string> GeneratePersonalizedRecommendationAsync(List<MediaItemInfo> items, string tone);
    
    bool IsConfigured();
}