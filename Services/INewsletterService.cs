using System.Threading.Tasks;

namespace Jellyfin.Plugin.AINewsletter.Services;

public interface INewsletterService
{
    Task<bool> GenerateAndSendNewsletterAsync();
    
    Task<string> GenerateNewsletterHtmlAsync();
    
    Task<bool> SendTestEmailAsync(string testRecipient);
}