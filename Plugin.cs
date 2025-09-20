using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.AINewsletter.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.AINewsletter;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public override string Name => "AI Newsletter";

    public override Guid Id => Guid.Parse("8B0B7F7A-3F8C-4F5D-9C2A-1E5F8B7A3C4D");

    public override string Description => "Generates AI-powered email newsletters featuring recently added media content with human-like descriptions and recommendations.";

    public static Plugin? Instance { get; private set; }

    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = this.Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace)
            }
        };
    }
}