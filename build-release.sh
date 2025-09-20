#!/bin/bash

# AI Newsletter Plugin - Release Build Script
# This script builds, packages, and prepares the plugin for release

set -e  # Exit on any error

# Configuration
VERSION="1.0.0.5"
PLUGIN_NAME="jellyfin-plugin-ainewsletter"
RELEASE_DIR="release"
BUILD_CONFIG="Release"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
print_header() {
    echo -e "${BLUE}================================================${NC}"
    echo -e "${BLUE}  Jellyfin AI Newsletter Plugin Release Build${NC}"
    echo -e "${BLUE}  Version: ${VERSION}${NC}"
    echo -e "${BLUE}================================================${NC}"
    echo
}

print_step() {
    echo -e "${YELLOW}[STEP]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

cleanup_build() {
    print_step "Cleaning previous build artifacts..."
    rm -rf bin obj
    rm -rf "${RELEASE_DIR}"
    print_success "Build artifacts cleaned"
}

build_plugin() {
    print_step "Building plugin in ${BUILD_CONFIG} mode..."
    dotnet clean --configuration ${BUILD_CONFIG}
    dotnet restore
    dotnet build --configuration ${BUILD_CONFIG} --no-restore
    print_success "Plugin built successfully"
}

create_release_dir() {
    print_step "Creating release directory..."
    mkdir -p "${RELEASE_DIR}"
    print_success "Release directory created"
}

package_plugin() {
    print_step "Packaging plugin binary..."
    local dll_path="bin/${BUILD_CONFIG}/net8.0/Jellyfin.Plugin.AINewsletter.dll"
    local zip_name="${PLUGIN_NAME}_${VERSION}.zip"

    if [ ! -f "${dll_path}" ]; then
        print_error "Plugin DLL not found at ${dll_path}"
        exit 1
    fi

    cp "${dll_path}" "${RELEASE_DIR}/"
    cd "${RELEASE_DIR}"
    zip "${zip_name}" Jellyfin.Plugin.AINewsletter.dll
    cd ..

    print_success "Plugin packaged as ${zip_name}"
}

create_source_archives() {
    print_step "Creating source code archives..."

    # Check if we're in a git repository
    if ! git rev-parse --git-dir > /dev/null 2>&1; then
        print_error "Not in a git repository. Skipping source archives."
        return
    fi

    local source_prefix="jellyfin-ai-newsletter-plugin-${VERSION}"

    # Create zip archive
    git archive --format=zip --prefix="${source_prefix}/" HEAD -o "${RELEASE_DIR}/${source_prefix}-source.zip"
    print_success "Source ZIP created: ${source_prefix}-source.zip"

    # Create tar.gz archive
    git archive --format=tar.gz --prefix="${source_prefix}/" HEAD -o "${RELEASE_DIR}/${source_prefix}-source.tar.gz"
    print_success "Source TAR.GZ created: ${source_prefix}-source.tar.gz"
}

calculate_checksums() {
    print_step "Calculating checksums..."
    cd "${RELEASE_DIR}"

    local zip_name="${PLUGIN_NAME}_${VERSION}.zip"

    if [ ! -f "${zip_name}" ]; then
        print_error "Plugin zip file not found: ${zip_name}"
        exit 1
    fi

    # Calculate MD5 checksum
    local checksum=""
    if command -v md5 >/dev/null 2>&1; then
        checksum=$(md5 -q "${zip_name}")
    elif command -v md5sum >/dev/null 2>&1; then
        checksum=$(md5sum "${zip_name}" | cut -d' ' -f1)
    else
        print_error "Neither md5 nor md5sum found. Cannot calculate checksum."
        exit 1
    fi

    echo "${checksum}" > "${zip_name}.md5"

    print_success "MD5 checksum: ${checksum}"
    print_success "Checksum saved to ${zip_name}.md5"

    # Create checksums for all files
    echo "# Jellyfin AI Newsletter Plugin v${VERSION} - File Checksums" > checksums.txt
    echo "# Generated on $(date)" >> checksums.txt
    echo "" >> checksums.txt

    for file in *.zip *.tar.gz *.dll; do
        if [ -f "$file" ]; then
            if command -v md5 >/dev/null 2>&1; then
                md5 "$file" >> checksums.txt
            elif command -v md5sum >/dev/null 2>&1; then
                md5sum "$file" >> checksums.txt
            fi
        fi
    done

    cd ..
    print_success "All checksums saved to checksums.txt"
}

generate_release_notes() {
    print_step "Generating release notes..."

    cat > "${RELEASE_DIR}/RELEASE_NOTES.md" << EOF
# Jellyfin AI Newsletter Plugin v${VERSION}

## 🚀 Features

### AI-Powered Content Generation
- **Smart Descriptions**: AI generates engaging, human-like descriptions that go beyond basic plot summaries
- **Multiple AI Providers**: Support for OpenAI, Anthropic Claude, and custom APIs
- **Personalized Tone**: Choose from friendly, professional, casual, enthusiastic, or cinephile writing styles
- **Custom Instructions**: Add your own prompts to customize the AI's writing style

### Modern Email Design
- **Responsive Templates**: Beautiful, mobile-friendly email layouts
- **Clean Formatting**: Professional design with proper typography and spacing
- **Image Support**: Include movie posters and album artwork
- **Multiple Hosting Options**: Jellyfin API or Imgur for image hosting

### Comprehensive Configuration
- **Flexible Scheduling**: Automated newsletter generation on your schedule
- **Library Filtering**: Choose which libraries and content types to include
- **SMTP Integration**: Full email delivery with SSL/TLS support
- **Multiple Recipients**: Send to multiple email addresses

### Smart Content Curation
- **Recent Content Focus**: Automatically finds content added within your specified timeframe
- **Content Type Filtering**: Movies, TV shows, music albums, books, and more
- **Rating Integration**: Include community ratings and metadata
- **Organized Sections**: Content automatically categorized by type

## 📦 Installation

1. Add this repository URL to your Jellyfin plugin catalog:
   \`https://raw.githubusercontent.com/nicklausroach/jellyfin-ai-newsletter-plugin/main/manifest.json\`

2. Or manually download and install:
   - Download \`${PLUGIN_NAME}_${VERSION}.zip\`
   - Extract to your Jellyfin plugins directory
   - Restart Jellyfin

## 🔧 Configuration

1. Navigate to Dashboard → Plugins → AI Newsletter
2. Configure your AI provider (OpenAI/Anthropic) with API key
3. Set up SMTP email settings
4. Choose content libraries and scheduling preferences
5. Test your configuration and enjoy automated newsletters!

## 📋 Requirements

- Jellyfin 10.9.0 or later
- AI API key (OpenAI, Anthropic, or compatible service)
- SMTP email server access

## 🐛 Support

- **Issues**: [GitHub Issues](https://github.com/nicklausroach/jellyfin-ai-newsletter-plugin/issues)
- **Documentation**: [README](https://github.com/nicklausroach/jellyfin-ai-newsletter-plugin/blob/main/README.md)

Generated on $(date)
EOF

    print_success "Release notes generated"
}

list_release_files() {
    print_step "Release package contents:"
    echo
    cd "${RELEASE_DIR}"
    ls -la
    echo
    print_success "Release build completed successfully!"
    echo
    echo -e "${BLUE}Files ready for GitHub release:${NC}"
    for file in *.zip *.tar.gz *.md *.txt; do
        if [ -f "$file" ]; then
            echo -e "  📦 ${file}"
        fi
    done
    cd ..
}

update_manifest_checksum() {
    print_step "Updating manifest.json with new checksum..."

    cd "${RELEASE_DIR}"
    local zip_name="${PLUGIN_NAME}_${VERSION}.zip"
    local checksum=""

    if command -v md5 >/dev/null 2>&1; then
        checksum=$(md5 -q "${zip_name}")
    elif command -v md5sum >/dev/null 2>&1; then
        checksum=$(md5sum "${zip_name}" | cut -d' ' -f1)
    fi

    cd ..

    # Note: You'll need to manually update the manifest.json with this checksum
    echo -e "${YELLOW}[NOTE]${NC} Update manifest.json with this checksum:"
    echo -e "${GREEN}${checksum}${NC}"
    echo
}

# Main execution
main() {
    print_header

    cleanup_build
    build_plugin
    create_release_dir
    package_plugin
    create_source_archives
    calculate_checksums
    generate_release_notes
    update_manifest_checksum
    list_release_files
}

# Run main function
main "$@"