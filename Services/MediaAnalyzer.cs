using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.AINewsletter.Configuration;
using Jellyfin.Plugin.AINewsletter.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AINewsletter.Services;

public class MediaAnalyzer : IMediaAnalyzer
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<MediaAnalyzer> _logger;
    private PluginConfiguration Configuration => Plugin.Instance?.Configuration ?? new PluginConfiguration();

    public MediaAnalyzer(ILibraryManager libraryManager, ILogger<MediaAnalyzer> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    public async Task<List<MediaItemInfo>> GetRecentlyAddedItemsAsync(DateTime since, string[] includedLibraries, string[] contentTypes, int maxItems)
    {
        try
        {
            var query = new InternalItemsQuery
            {
                MinDateCreated = since,
                Recursive = true,
                OrderBy = new[] { (ItemSortBy.DateCreated, SortOrder.Descending) },
                Limit = maxItems * 2, // Get more items to filter, in case some don't meet criteria
                IncludeItemTypes = GetIncludeItemTypes(contentTypes)
            };

            var items = _libraryManager.GetItemList(query);
            var filteredItems = FilterItemsByLibrary(items, includedLibraries);
            var mediaItemInfos = new List<MediaItemInfo>();

            foreach (var item in filteredItems.Take(maxItems))
            {
                try
                {
                    var mediaInfo = ConvertToMediaItemInfo(item);
                    if (Configuration.IncludePosters)
                    {
                        mediaInfo.PosterUrl = await GetPosterUrlAsync(item);
                    }
                    mediaItemInfos.Add(mediaInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to convert item {ItemName} to MediaItemInfo", item.Name);
                }
            }

            _logger.LogInformation("Found {ItemCount} recently added items since {SinceDate}", mediaItemInfos.Count, since);
            return mediaItemInfos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recently added items");
            return new List<MediaItemInfo>();
        }
    }

    public MediaItemInfo ConvertToMediaItemInfo(BaseItem item)
    {
        var mediaInfo = new MediaItemInfo
        {
            Id = item.Id.ToString(),
            Title = item.Name ?? "Unknown Title",
            Type = GetItemTypeName(item),
            Year = item.ProductionYear,
            Overview = item.Overview,
            Genres = item.Genres?.ToArray() ?? Array.Empty<string>(),
            Rating = item.OfficialRating,
            CommunityRating = item.CommunityRating,
            DateAdded = item.DateCreated,
            Library = item.GetParent()?.Name
        };

        // Add type-specific information
        switch (item)
        {
            case Movie:
                // People information would be populated separately if needed
                // For now, leave these fields empty as they require additional API calls
                break;

            case Series:
                // People information would be populated separately if needed
                break;

            case Season season:
                mediaInfo.SeriesName = season.Series?.Name;
                mediaInfo.SeasonNumber = season.IndexNumber;
                break;

            case Episode episode:
                mediaInfo.SeriesName = episode.Series?.Name;
                mediaInfo.SeasonNumber = episode.Season?.IndexNumber;
                mediaInfo.EpisodeNumber = episode.IndexNumber;
                break;

            case MusicAlbum album:
                mediaInfo.AlbumArtist = album.AlbumArtists?.FirstOrDefault();
                mediaInfo.TrackCount = album.Children?.Count();
                break;

            case Audio audio:
                mediaInfo.AlbumArtist = audio.AlbumArtists?.FirstOrDefault();
                break;
        }

        return mediaInfo;
    }

    public async Task<string?> GetPosterUrlAsync(BaseItem item)
    {
        try
        {
            if (Configuration.PosterHostingType == "Imgur")
            {
                return await GetImgurPosterUrlAsync(item);
            }
            else
            {
                return GetJellyfinPosterUrl(item);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get poster URL for item {ItemName}", item.Name);
            return null;
        }
    }

    private async Task<string?> GetImgurPosterUrlAsync(BaseItem item)
    {
        // Imgur implementation would go here
        // For now, fallback to Jellyfin API
        await Task.CompletedTask;
        return GetJellyfinPosterUrl(item);
    }

    private string? GetJellyfinPosterUrl(BaseItem item)
    {
        if (!item.HasImage(MediaBrowser.Model.Entities.ImageType.Primary))
        {
            return null;
        }

        // This would need to be constructed based on your Jellyfin server URL
        // For now, return a placeholder that can be replaced in the email template
        return $"/Items/{item.Id}/Images/Primary";
    }

    private BaseItemKind[] GetIncludeItemTypes(string[] contentTypes)
    {
        var includeTypes = new List<BaseItemKind>();

        foreach (var type in contentTypes)
        {
            switch (type.ToLowerInvariant())
            {
                case "movie":
                    includeTypes.Add(BaseItemKind.Movie);
                    break;
                case "series":
                    includeTypes.Add(BaseItemKind.Series);
                    break;
                case "season":
                    includeTypes.Add(BaseItemKind.Season);
                    break;
                case "episode":
                    includeTypes.Add(BaseItemKind.Episode);
                    break;
                case "musicalbum":
                    includeTypes.Add(BaseItemKind.MusicAlbum);
                    break;
                case "audio":
                    includeTypes.Add(BaseItemKind.Audio);
                    break;
                case "book":
                    includeTypes.Add(BaseItemKind.Book);
                    break;
            }
        }

        return includeTypes.ToArray();
    }

    private string GetItemTypeName(BaseItem item)
    {
        return item switch
        {
            Movie => "Movie",
            Series => "Series",
            Season => "Season",
            Episode => "Episode",
            MusicAlbum => "MusicAlbum",
            Audio => "Audio",
            Book => "Book",
            _ => item.GetClientTypeName()
        };
    }

    private List<BaseItem> FilterItemsByLibrary(List<BaseItem> items, string[] includedLibraries)
    {
        if (includedLibraries == null || includedLibraries.Length == 0)
        {
            return items;
        }

        return items.Where(item =>
        {
            var libraryName = item.GetParent()?.Name;
            return libraryName != null && includedLibraries.Contains(libraryName, StringComparer.OrdinalIgnoreCase);
        }).ToList();
    }
}