using System.Threading.Tasks;
using Jellyfin.Plugin.AINewsletter.Models;

namespace Jellyfin.Plugin.AINewsletter.Services;

public interface ITemplateService
{
    Task<string> GenerateEmailHtmlAsync(NewsletterContent content);
    
    string GetDefaultTemplate();
}