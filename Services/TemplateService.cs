using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Plugin.AINewsletter.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AINewsletter.Services;

public class TemplateService : ITemplateService
{
    private readonly ILogger<TemplateService> _logger;
    private string? _cachedTemplate;

    public TemplateService(ILogger<TemplateService> logger)
    {
        _logger = logger;
    }

    public async Task<string> GenerateEmailHtmlAsync(NewsletterContent content)
    {
        try
        {
            var template = await GetTemplateAsync();
            
            var html = template
                .Replace("{{NEWSLETTER_TITLE}}", content.Title)
                .Replace("{{GENERATION_DATE}}", content.GeneratedAt.ToString("MMMM dd, yyyy"))
                .Replace("{{INTRODUCTION}}", content.Introduction)
                .Replace("{{CONCLUSION}}", content.Conclusion)
                .Replace("{{SECTIONS}}", GenerateSectionsHtml(content.Sections));

            return html;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate email HTML from template");
            return GenerateFallbackHtml(content);
        }
    }

    public string GetDefaultTemplate()
    {
        return GetTemplateAsync().GetAwaiter().GetResult();
    }

    private async Task<string> GetTemplateAsync()
    {
        if (_cachedTemplate != null)
        {
            return _cachedTemplate;
        }

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(r => r.EndsWith("newsletter-template.html"));

            if (resourceName != null)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    _cachedTemplate = await reader.ReadToEndAsync();
                    return _cachedTemplate;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load embedded template, using built-in template");
        }

        // Fallback to built-in template
        _cachedTemplate = GetBuiltInTemplate();
        return _cachedTemplate;
    }

    private string GenerateSectionsHtml(System.Collections.Generic.List<NewsletterSection> sections)
    {
        var sectionsHtml = new StringBuilder();

        foreach (var section in sections)
        {
            sectionsHtml.AppendLine($@"
            <div class=""section"">
                <div class=""section-header"">
                    <div>
                        <h2 class=""section-title"">{EscapeHtml(section.SectionTitle)}</h2>
                        <p class=""section-description"">{EscapeHtml(section.Description)}</p>
                    </div>
                </div>
                <div class=""media-items"">
                    {GenerateMediaItemsHtml(section.Items)}
                </div>
            </div>");
        }

        return sectionsHtml.ToString();
    }

    private string GenerateMediaItemsHtml(System.Collections.Generic.List<MediaItemInfo> items)
    {
        var itemsHtml = new StringBuilder();

        foreach (var item in items)
        {
            var posterHtml = !string.IsNullOrEmpty(item.PosterUrl)
                ? $@"<img src=""{EscapeHtml(item.PosterUrl)}"" alt=""{EscapeHtml(item.Title)} poster"" />"
                : $@"<div class=""media-poster-placeholder"">No Image<br/>Available</div>";

            var metaItems = new System.Collections.Generic.List<string>();
            
            if (item.Year.HasValue)
                metaItems.Add($"<span>{item.Year}</span>");
            
            if (!string.IsNullOrEmpty(item.Type))
                metaItems.Add($"<span>{EscapeHtml(item.Type)}</span>");
            
            if (item.CommunityRating.HasValue)
                metaItems.Add($"<span class=\"rating\">★ {item.CommunityRating:F1}</span>");
            
            if (!string.IsNullOrEmpty(item.Director))
                metaItems.Add($"<span>Dir: {EscapeHtml(item.Director)}</span>");

            var genresHtml = item.Genres.Any() 
                ? string.Join("", item.Genres.Take(3).Select(g => $@"<span class=""genre-tag"">{EscapeHtml(g)}</span>"))
                : "";

            itemsHtml.AppendLine($@"
            <div class=""media-item"">
                <div class=""media-poster"">
                    {posterHtml}
                </div>
                <div class=""media-details"">
                    <h3 class=""media-title"">{EscapeHtml(item.Title)}</h3>
                    <div class=""media-meta"">
                        {string.Join("", metaItems)}
                    </div>
                    {(!string.IsNullOrEmpty(item.Overview) ? $@"<p class=""media-overview"">{EscapeHtml(item.Overview)}</p>" : "")}
                    <div class=""media-genres"">
                        {genresHtml}
                    </div>
                </div>
            </div>");
        }

        return itemsHtml.ToString();
    }

    private string GenerateFallbackHtml(NewsletterContent content)
    {
        var html = new StringBuilder();
        
        html.AppendLine($@"
        <html>
        <head>
            <title>{EscapeHtml(content.Title)}</title>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 20px; line-height: 1.6; }}
                .header {{ background: #667eea; color: white; padding: 20px; text-align: center; }}
                .content {{ padding: 20px; }}
                .item {{ margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 8px; }}
                .title {{ font-size: 18px; font-weight: bold; color: #333; }}
                .meta {{ color: #666; font-size: 14px; margin: 5px 0; }}
            </style>
        </head>
        <body>
            <div class=""header"">
                <h1>{EscapeHtml(content.Title)}</h1>
                <p>Generated on {content.GeneratedAt:MMMM dd, yyyy}</p>
            </div>
            <div class=""content"">
                <p>{EscapeHtml(content.Introduction)}</p>");

        foreach (var section in content.Sections)
        {
            html.AppendLine($@"
                <h2>{EscapeHtml(section.SectionTitle)}</h2>
                <p><em>{EscapeHtml(section.Description)}</em></p>");

            foreach (var item in section.Items)
            {
                html.AppendLine($@"
                <div class=""item"">
                    <div class=""title"">{EscapeHtml(item.Title)}</div>
                    <div class=""meta"">{EscapeHtml(item.Type)}{(item.Year.HasValue ? $" ({item.Year})" : "")}</div>
                    {(!string.IsNullOrEmpty(item.Overview) ? $"<p>{EscapeHtml(item.Overview)}</p>" : "")}
                </div>");
            }
        }

        html.AppendLine($@"
                <p>{EscapeHtml(content.Conclusion)}</p>
            </div>
        </body>
        </html>");

        return html.ToString();
    }

    private string GetBuiltInTemplate()
    {
        return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{{NEWSLETTER_TITLE}}</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; margin: 0; padding: 0; background: #f8f9fa; }
        .container { max-width: 600px; margin: 0 auto; background: white; }
        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 40px 30px; text-align: center; }
        .content { padding: 30px; }
        .introduction { background: #f8f9fa; padding: 20px; border-radius: 8px; border-left: 4px solid #667eea; margin-bottom: 30px; }
        .section { margin-bottom: 40px; }
        .section-title { font-size: 22px; color: #343a40; margin-bottom: 20px; }
        .media-item { display: flex; background: white; border-radius: 12px; box-shadow: 0 2px 8px rgba(0,0,0,0.08); margin-bottom: 20px; overflow: hidden; }
        .media-details { padding: 20px; flex: 1; }
        .media-title { font-size: 18px; font-weight: 600; margin-bottom: 8px; }
        .conclusion { background: #f8f9fa; padding: 25px; border-radius: 8px; text-align: center; border-left: 4px solid #28a745; }
        .footer { background: #343a40; color: white; padding: 25px; text-align: center; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>{{NEWSLETTER_TITLE}}</h1>
            <p>Generated on {{GENERATION_DATE}}</p>
        </div>
        <div class=""content"">
            <div class=""introduction"">{{INTRODUCTION}}</div>
            {{SECTIONS}}
            <div class=""conclusion""><p>{{CONCLUSION}}</p></div>
        </div>
        <div class=""footer"">
            <p>Generated by <strong>Jellyfin AI Newsletter Plugin</strong></p>
        </div>
    </div>
</body>
</html>";
    }

    private static string EscapeHtml(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}