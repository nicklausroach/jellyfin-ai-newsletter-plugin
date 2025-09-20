using System.Threading.Tasks;

namespace Jellyfin.Plugin.AINewsletter.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string recipient, string subject, string htmlBody);
    
    Task<bool> TestConnectionAsync();
    
    bool IsConfigured();
}