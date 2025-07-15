#!/bin/bash

# Universal build script for all platforms
# Taskmaster Project

set -e

echo "🚀 Building Taskmaster for all platforms..."
echo "============================================"

# Check .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "❌ Error: .NET SDK not found"
    echo "Please install .NET 8.0 SDK from https://dotnet.microsoft.com/"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo "✅ Found .NET SDK: $DOTNET_VERSION"

# Clean all previous builds
echo "🧹 Cleaning previous builds..."
rm -rf bin/Release/
rm -rf publish/
rm -rf release/

# Create release directory structure
mkdir -p release

echo ""
echo "📦 Building for multiple platforms..."
echo "====================================="

# Build Linux x64
echo ""
echo "🐧 Building for Linux x64..."
./build-linux.sh

# Build macOS
echo ""
echo "🍎 Building for macOS..."
./build-macos.sh

# Build Windows x64
echo ""
echo "🪟 Building for Windows x64..."
./build-windows.sh

# Create distribution packages
echo ""
echo "📦 Creating distribution packages..."

cd release

# Create tar.gz for Linux
if [ -d "linux-x64" ]; then
    echo "Creating taskmaster-linux-x64.tar.gz..."
    tar -czf taskmaster-linux-x64.tar.gz linux-x64/
fi

# Create tar.gz for macOS
if [ -d "osx-arm64" ]; then
    echo "Creating taskmaster-macos-arm64.tar.gz..."
    tar -czf taskmaster-macos-arm64.tar.gz osx-arm64/
fi

if [ -d "osx-x64" ]; then
    echo "Creating taskmaster-macos-x64.tar.gz..."
    tar -czf taskmaster-macos-x64.tar.gz osx-x64/
fi

# Create zip for Windows
if [ -d "win-x64" ]; then
    if command -v zip &> /dev/null; then
        echo "Creating taskmaster-windows-x64.zip..."
        zip -r taskmaster-windows-x64.zip win-x64/
    else
        echo "⚠️  Warning: zip command not found, skipping Windows package"
    fi
fi

cd ..

echo ""
echo "🎉 All builds completed successfully!"
echo "===================================="
echo ""
echo "📁 Release files:"
ls -la release/

echo ""
echo "📋 Platform Support:"
echo "  🐧 Linux x64    - Self-contained binary"
echo "  🍎 macOS ARM64  - Apple Silicon"
echo "  🍎 macOS x64    - Intel Mac"
echo "  🪟 Windows x64  - Self-contained binary"
echo ""
echo "🚀 Run Instructions:"
echo "  Linux:   ./release/linux-x64/Taskmaster"
echo "  macOS:   ./release/osx-*/Taskmaster"
echo "  Windows: release\\win-x64\\Taskmaster.exe"
echo ""
echo "✨ Distribution packages created in release/ directory"