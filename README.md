# Jellyfin AI Newsletter Plugin

An intelligent Jellyfin plugin that generates AI-powered email newsletters featuring recently added media content with human-like descriptions and personalized recommendations.

## Features

### 🤖 AI-Powered Content Generation
- **Smart Descriptions**: AI generates engaging, human-like descriptions that go beyond basic plot summaries
- **Multiple AI Providers**: Support for OpenAI, Anthropic Claude, and custom APIs
- **Personalized Tone**: Choose from friendly, professional, casual, enthusiastic, or cinephile writing styles
- **Custom Instructions**: Add your own prompts to customize the AI's writing style

### 📧 Modern Email Design
- **Responsive Templates**: Beautiful, mobile-friendly email layouts
- **Clean Formatting**: Professional design with proper typography and spacing
- **Image Support**: Include movie posters and album artwork
- **Multiple Hosting Options**: Jellyfin API or Imgur for image hosting

### ⚙️ Comprehensive Configuration
- **Flexible Scheduling**: Automated newsletter generation on your schedule
- **Library Filtering**: Choose which libraries and content types to include
- **SMTP Integration**: Full email delivery with SSL/TLS support
- **Multiple Recipients**: Send to multiple email addresses

### 🎯 Smart Content Curation
- **Recent Content Focus**: Automatically finds content added within your specified timeframe
- **Content Type Filtering**: Movies, TV shows, music albums, books, and more
- **Rating Integration**: Include community ratings and metadata
- **Organized Sections**: Content automatically categorized by type

## Installation

### From Jellyfin Plugin Catalog (Recommended)
1. Open your Jellyfin admin dashboard
2. Navigate to **Plugins** → **Catalog**
3. Search for "AI Newsletter"
4. Click **Install**
5. Restart your Jellyfin server

### Manual Installation
1. Download the latest release from the [releases page](https://github.com/nicklausroach/jellyfin-ai-newsletter-plugin/releases)
2. Extract the plugin files to your Jellyfin plugins directory
3. Restart your Jellyfin server

## Configuration

### 1. AI Service Setup
1. Go to **Dashboard** → **Plugins** → **AI Newsletter**
2. Choose your AI provider (OpenAI or Anthropic recommended)
3. Enter your API key
4. Select your preferred model and tone
5. Optionally add custom instructions

### 2. Email Configuration
1. Configure your SMTP server settings
2. Set sender email and name
3. Add recipient email addresses (one per line)
4. Test the connection

### 3. Content Settings
1. Choose which libraries to include
2. Select content types (Movies, TV Shows, Music, etc.)
3. Set how far back to scan for new content
4. Configure maximum items per newsletter

### 4. Scheduling
1. Enable scheduled newsletter generation
2. Set the frequency (daily, weekly, etc.)
3. The plugin will automatically run based on your schedule

## API Provider Setup

### OpenAI
1. Create an account at [OpenAI](https://platform.openai.com/)
2. Generate an API key from your dashboard
3. Recommended models: `gpt-4o-mini` or `gpt-4o`

### Anthropic Claude
1. Create an account at [Anthropic](https://console.anthropic.com/)
2. Generate an API key
3. Recommended models: `claude-3-haiku-20240307` or `claude-3-sonnet-20240229`

### Custom API
For other OpenAI-compatible APIs, use the "Custom" provider option and specify your API endpoint.

## SMTP Configuration Examples

### Gmail
- **Server**: smtp.gmail.com
- **Port**: 587
- **SSL**: Enabled
- **Username**: your-email@gmail.com
- **Password**: Use an [App Password](https://support.google.com/accounts/answer/185833)

### Outlook/Hotmail
- **Server**: smtp-mail.outlook.com
- **Port**: 587
- **SSL**: Enabled
- **Username**: your-email@outlook.com
- **Password**: Your account password

### Custom SMTP
Configure according to your email provider's documentation.

## Troubleshooting

### Newsletter Not Generating
1. Check that you have new content in the specified timeframe
2. Verify your AI API key is valid and has credits
3. Ensure at least one library and content type is selected
4. Check the Jellyfin logs for error messages

### Emails Not Sending
1. Test your SMTP connection in the plugin settings
2. Verify your SMTP credentials and server settings
3. Check that your email provider allows SMTP access
4. Ensure recipient email addresses are valid

### AI Responses Seem Off
1. Try adjusting the newsletter tone setting
2. Add custom instructions to guide the AI
3. Check that your API key has sufficient credits
4. Consider switching to a different AI model

## Development

### Requirements
- .NET 8.0 SDK
- Jellyfin 10.9.0 or later

### Building
```bash
dotnet build
```

### Testing
```bash
dotnet test
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the GPL-3.0 License - see the [LICENSE](LICENSE) file for details.

## Support

- **Issues**: [GitHub Issues](https://github.com/nicklausroach/jellyfin-ai-newsletter-plugin/issues)
- **Discussions**: [GitHub Discussions](https://github.com/nicklausroach/jellyfin-ai-newsletter-plugin/discussions)
- **Jellyfin Community**: [Jellyfin Discord](https://jellyfin.org/contact)

## Acknowledgments

- Inspired by the original [Jellyfin Newsletter Plugin](https://github.com/Cloud9Developer/Jellyfin-Newsletter-Plugin)
- Built for the Jellyfin media server community
- Powered by AI for enhanced content descriptions