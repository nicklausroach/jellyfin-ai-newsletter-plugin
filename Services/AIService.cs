using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Plugin.AINewsletter.Configuration;
using Jellyfin.Plugin.AINewsletter.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AINewsletter.Services;

public class AIService : IAIService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AIService> _logger;
    private PluginConfiguration Configuration => Plugin.Instance?.Configuration ?? new PluginConfiguration();

    public AIService(IHttpClientFactory httpClientFactory, ILogger<AIService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(Configuration.AIApiKey) && !string.IsNullOrEmpty(Configuration.AIProvider);
    }

    public async Task<NewsletterContent> GenerateNewsletterAsync(List<MediaItemInfo> mediaItems, string tone, bool enablePersonalization, string? customPrompt = null)
    {
        if (!IsConfigured())
        {
            throw new InvalidOperationException("AI service is not properly configured. Please check your API key and provider settings.");
        }

        try
        {
            var prompt = BuildNewsletterPrompt(mediaItems, tone, enablePersonalization, customPrompt);
            var response = await CallAIAsync(prompt);
            
            return ParseNewsletterResponse(response, mediaItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate newsletter content");
            return CreateFallbackNewsletter(mediaItems);
        }
    }

    public async Task<string> GenerateItemDescriptionAsync(MediaItemInfo item, string tone)
    {
        if (!IsConfigured())
        {
            return item.Overview ?? $"New {item.Type.ToLower()}: {item.Title}";
        }

        try
        {
            var prompt = BuildItemDescriptionPrompt(item, tone);
            return await CallAIAsync(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate item description for {Title}", item.Title);
            return item.Overview ?? $"New {item.Type.ToLower()}: {item.Title}";
        }
    }

    public async Task<string> GeneratePersonalizedRecommendationAsync(List<MediaItemInfo> items, string tone)
    {
        if (!IsConfigured())
        {
            return "Check out these great new additions to your library!";
        }

        try
        {
            var prompt = BuildRecommendationPrompt(items, tone);
            return await CallAIAsync(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate personalized recommendation");
            return "Check out these great new additions to your library!";
        }
    }

    private async Task<string> CallAIAsync(string prompt)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        
        return Configuration.AIProvider.ToLowerInvariant() switch
        {
            "openai" => await CallOpenAIAsync(httpClient, prompt),
            "anthropic" => await CallAnthropicAsync(httpClient, prompt),
            "custom" => await CallCustomAPIAsync(httpClient, prompt),
            _ => throw new NotSupportedException($"AI provider '{Configuration.AIProvider}' is not supported")
        };
    }

    private async Task<string> CallOpenAIAsync(HttpClient httpClient, string prompt)
    {
        var request = new OpenAIRequest
        {
            Model = Configuration.AIModel,
            Messages = new List<OpenAIMessage>
            {
                new() { Role = "system", Content = "You are a helpful assistant that creates engaging newsletter content about movies, TV shows, and music. Be creative, engaging, and write in a human-like style." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.7f,
            MaxTokens = 2000
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Configuration.AIApiKey}");
        
        var response = await httpClient.PostAsync($"{Configuration.AIBaseUrl}/chat/completions", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseJson);

        return openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? throw new Exception("Invalid OpenAI response");
    }

    private async Task<string> CallAnthropicAsync(HttpClient httpClient, string prompt)
    {
        var request = new AnthropicRequest
        {
            Model = Configuration.AIModel,
            MaxTokens = 2000,
            Messages = new List<AnthropicMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.7f
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        httpClient.DefaultRequestHeaders.Add("x-api-key", Configuration.AIApiKey);
        httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        
        var response = await httpClient.PostAsync($"{Configuration.AIBaseUrl}/messages", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var anthropicResponse = JsonSerializer.Deserialize<AnthropicResponse>(responseJson);

        return anthropicResponse?.Content?.FirstOrDefault()?.Text ?? throw new Exception("Invalid Anthropic response");
    }

    private async Task<string> CallCustomAPIAsync(HttpClient httpClient, string prompt)
    {
        var request = new OpenAIRequest
        {
            Model = Configuration.AIModel,
            Messages = new List<OpenAIMessage>
            {
                new() { Role = "system", Content = "You are a helpful assistant that creates engaging newsletter content." },
                new() { Role = "user", Content = prompt }
            }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        if (!string.IsNullOrEmpty(Configuration.AIApiKey))
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Configuration.AIApiKey}");
        }
        
        var response = await httpClient.PostAsync($"{Configuration.AIBaseUrl}/chat/completions", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseJson);

        return openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? throw new Exception("Invalid API response");
    }

    private string BuildNewsletterPrompt(List<MediaItemInfo> items, string tone, bool enablePersonalization, string? customPrompt)
    {
        var itemsText = string.Join("\n", items.Select(FormatItemForPrompt));
        
        var basePrompt = $@"Create an engaging email newsletter featuring these recently added media items. 

TONE: {tone}
PERSONALIZATION: {(enablePersonalization ? "Enabled - add personal touches and recommendations" : "Disabled - keep it general")}

MEDIA ITEMS:
{itemsText}

Please create a newsletter with:
1. An engaging subject line and introduction
2. Organized sections for different content types (Movies, TV Shows, Music, etc.)
3. Brief, enticing descriptions for each item that go beyond the basic plot summary
4. {(enablePersonalization ? "Personalized recommendations and viewing suggestions" : "General recommendations")}
5. A warm conclusion encouraging engagement

Make it feel like it's written by a human who's genuinely excited about these additions. Avoid overly promotional language.
Format the response as JSON with this structure:
{{
  ""title"": ""Newsletter title"",
  ""introduction"": ""Welcome paragraph"",
  ""sections"": [
    {{
      ""sectionTitle"": ""Section name"",
      ""description"": ""Section description"",
      ""items"": [/* include the media items for this section */]
    }}
  ],
  ""conclusion"": ""Closing paragraph""
}}";

        if (!string.IsNullOrEmpty(customPrompt))
        {
            basePrompt += $"\n\nADDITIONAL INSTRUCTIONS: {customPrompt}";
        }

        return basePrompt;
    }

    private string BuildItemDescriptionPrompt(MediaItemInfo item, string tone)
    {
        return $@"Create a brief, engaging description for this {item.Type.ToLower()}:

Title: {item.Title}
{(item.Year.HasValue ? $"Year: {item.Year}" : "")}
{(!string.IsNullOrEmpty(item.Overview) ? $"Overview: {item.Overview}" : "")}
{(item.Genres.Any() ? $"Genres: {string.Join(", ", item.Genres)}" : "")}
{(!string.IsNullOrEmpty(item.Director) ? $"Director: {item.Director}" : "")}

Tone: {tone}

Write a 1-2 sentence description that's more engaging than the basic plot summary. Focus on what makes this content interesting or appealing to watch.";
    }

    private string BuildRecommendationPrompt(List<MediaItemInfo> items, string tone)
    {
        var itemsList = string.Join("\n", items.Select(i => $"- {i.Title} ({i.Type})"));
        
        return $@"Based on these recently added items, write a brief personalized recommendation paragraph:

{itemsList}

Tone: {tone}

Create a warm, engaging paragraph that highlights why these additions are worth checking out. Make it feel personal and genuine.";
    }

    private string FormatItemForPrompt(MediaItemInfo item)
    {
        var details = new List<string> { $"Title: {item.Title}", $"Type: {item.Type}" };
        
        if (item.Year.HasValue) details.Add($"Year: {item.Year}");
        if (!string.IsNullOrEmpty(item.Overview)) details.Add($"Overview: {item.Overview}");
        if (item.Genres.Any()) details.Add($"Genres: {string.Join(", ", item.Genres)}");
        if (!string.IsNullOrEmpty(item.Director)) details.Add($"Director: {item.Director}");
        if (item.CommunityRating.HasValue) details.Add($"Rating: {item.CommunityRating:F1}/10");
        
        return string.Join("\n", details) + "\n";
    }

    private NewsletterContent ParseNewsletterResponse(string response, List<MediaItemInfo> originalItems)
    {
        try
        {
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart);
                var parsed = JsonSerializer.Deserialize<NewsletterContent>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (parsed != null)
                {
                    AssignItemsToSections(parsed, originalItems);
                    return parsed;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse AI response as JSON, creating structured content from text");
        }

        return CreateStructuredContentFromText(response, originalItems);
    }

    private void AssignItemsToSections(NewsletterContent content, List<MediaItemInfo> originalItems)
    {
        var movieItems = originalItems.Where(i => i.Type == "Movie").ToList();
        var tvItems = originalItems.Where(i => i.Type == "Series" || i.Type == "Season" || i.Type == "Episode").ToList();
        var musicItems = originalItems.Where(i => i.Type == "MusicAlbum" || i.Type == "Audio").ToList();
        var otherItems = originalItems.Except(movieItems).Except(tvItems).Except(musicItems).ToList();

        foreach (var section in content.Sections)
        {
            var sectionType = section.SectionTitle.ToLowerInvariant();
            
            if (sectionType.Contains("movie") || sectionType.Contains("film"))
            {
                section.Items = movieItems;
            }
            else if (sectionType.Contains("tv") || sectionType.Contains("series") || sectionType.Contains("show"))
            {
                section.Items = tvItems;
            }
            else if (sectionType.Contains("music") || sectionType.Contains("album") || sectionType.Contains("audio"))
            {
                section.Items = musicItems;
            }
            else if (otherItems.Any())
            {
                section.Items = otherItems;
            }
        }
    }

    private NewsletterContent CreateStructuredContentFromText(string text, List<MediaItemInfo> items)
    {
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        return new NewsletterContent
        {
            Title = "Your Weekly Media Update",
            Introduction = lines.Length > 0 ? lines[0] : "Check out what's new in your library!",
            Sections = new List<NewsletterSection>
            {
                new()
                {
                    SectionTitle = "New Additions",
                    Description = "Recently added content",
                    Items = items
                }
            },
            Conclusion = "Enjoy watching and listening!"
        };
    }

    private NewsletterContent CreateFallbackNewsletter(List<MediaItemInfo> items)
    {
        _logger.LogWarning("Creating fallback newsletter content due to AI service failure");
        
        var movieItems = items.Where(i => i.Type == "Movie").ToList();
        var tvItems = items.Where(i => i.Type == "Series" || i.Type == "Season" || i.Type == "Episode").ToList();
        var musicItems = items.Where(i => i.Type == "MusicAlbum" || i.Type == "Audio").ToList();
        var otherItems = items.Except(movieItems).Except(tvItems).Except(musicItems).ToList();

        var sections = new List<NewsletterSection>();
        
        if (movieItems.Any())
        {
            sections.Add(new NewsletterSection
            {
                SectionTitle = "🎬 New Movies",
                Description = "Fresh movies added to your collection",
                Items = movieItems
            });
        }
        
        if (tvItems.Any())
        {
            sections.Add(new NewsletterSection
            {
                SectionTitle = "📺 TV Shows & Episodes", 
                Description = "New series and episodes to binge",
                Items = tvItems
            });
        }
        
        if (musicItems.Any())
        {
            sections.Add(new NewsletterSection
            {
                SectionTitle = "🎵 New Music",
                Description = "Latest albums and tracks",
                Items = musicItems
            });
        }
        
        if (otherItems.Any())
        {
            sections.Add(new NewsletterSection
            {
                SectionTitle = "📚 Other Content",
                Description = "Additional new content",
                Items = otherItems
            });
        }

        return new NewsletterContent
        {
            Title = "Your Weekly Jellyfin Update",
            Introduction = $"We've got {items.Count} new {(items.Count == 1 ? "item" : "items")} in your library this week!",
            Sections = sections,
            Conclusion = "Happy watching and listening! Your Jellyfin server has been busy adding great new content for you to enjoy."
        };
    }
}