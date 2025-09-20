using Jellyfin.Plugin.AINewsletter.Services;
using Jellyfin.Plugin.AINewsletter.Tasks;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.AINewsletter;

public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        // Register all our services
        serviceCollection.AddScoped<IMediaAnalyzer, MediaAnalyzer>();
        serviceCollection.AddScoped<IAIService, AIService>();
        serviceCollection.AddScoped<IEmailService, EmailService>();
        serviceCollection.AddScoped<ITemplateService, TemplateService>();
        serviceCollection.AddScoped<INewsletterService, NewsletterService>();
        
        // Register scheduled task
        serviceCollection.AddScoped<NewsletterScheduledTask>();
        
        // Add HTTP client factory for AI service calls
        serviceCollection.AddHttpClient();
    }
}