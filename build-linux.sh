#!/bin/bash

# Build script for Linux x64
# Taskmaster Project

set -e

echo "Building Taskmaster for Linux x64..."

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf bin/Release/net8.0/linux-x64/
rm -rf publish/linux-x64/

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

# Build for Linux x64
echo "Building for Linux x64..."
dotnet publish -c Release -r linux-x64 --self-contained true -o publish/linux-x64/ \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:IncludeNativeLibrariesForSelfExtract=true

# Compile test programs
echo "Compiling test programs..."
cd tests
g++ -static -o simple_output simple_output.cpp
g++ -static -o heavy_output heavy_output.cpp  
g++ -static -o failing_process failing_process.cpp
g++ -static -o signal_handler signal_handler.cpp
g++ -static -o env_test env_test.cpp
g++ -static -o quick_exit quick_exit.cpp
g++ -static -o long_startup long_startup.cpp
cd ..

# Create release directory
mkdir -p release/linux-x64
cp publish/linux-x64/Taskmaster release/linux-x64/
cp -r tests/ release/linux-x64/
cp *.config.json release/linux-x64/ 2>/dev/null || true
cp subject.md release/linux-x64/ 2>/dev/null || true
cp README.md release/linux-x64/ 2>/dev/null || true

# Set executable permissions
chmod +x release/linux-x64/Taskmaster
chmod +x release/linux-x64/tests/simple_output
chmod +x release/linux-x64/tests/heavy_output
chmod +x release/linux-x64/tests/failing_process
chmod +x release/linux-x64/tests/signal_handler
chmod +x release/linux-x64/tests/env_test
chmod +x release/linux-x64/tests/quick_exit
chmod +x release/linux-x64/tests/long_startup

echo "✅ Linux x64 build completed!"
echo "📁 Release files in: release/linux-x64/"
echo "🚀 Run with: ./release/linux-x64/Taskmaster"