using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.Plugin.AINewsletter.Models;
using MediaBrowser.Controller.Entities;

namespace Jellyfin.Plugin.AINewsletter.Services;

public interface IMediaAnalyzer
{
    Task<List<MediaItemInfo>> GetRecentlyAddedItemsAsync(DateTime since, string[] includedLibraries, string[] contentTypes, int maxItems);
    
    MediaItemInfo ConvertToMediaItemInfo(BaseItem item);
    
    Task<string?> GetPosterUrlAsync(BaseItem item);
}