#!/bin/bash

# Build script for macOS (ARM64 and x64)
# Taskmaster Project

set -e

echo "Building Taskmaster for macOS..."

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf bin/Release/net8.0/osx-*
rm -rf publish/osx-*

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

# Detect macOS architecture
ARCH=$(uname -m)
if [[ "$ARCH" == "arm64" ]]; then
    TARGET="osx-arm64"
    echo "Detected Apple Silicon (ARM64)"
else
    TARGET="osx-x64"
    echo "Detected Intel x64"
fi

# Build for detected architecture
echo "Building for $TARGET..."
dotnet publish -c Release -r $TARGET --self-contained true -o publish/$TARGET/ \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:IncludeNativeLibrariesForSelfExtract=true

# Compile test programs
echo "Compiling test programs..."
cd tests

# Check if we can use clang++ or g++
if command -v clang++ &> /dev/null; then
    CXX="clang++"
    echo "Using clang++ compiler"
elif command -v g++ &> /dev/null; then
    CXX="g++"
    echo "Using g++ compiler"
else
    echo "❌ Error: No C++ compiler found (clang++ or g++)"
    exit 1
fi

$CXX -std=c++11 -o simple_output simple_output.cpp
$CXX -std=c++11 -o heavy_output heavy_output.cpp  
$CXX -std=c++11 -o failing_process failing_process.cpp
$CXX -std=c++11 -o signal_handler signal_handler.cpp
$CXX -std=c++11 -o env_test env_test.cpp
$CXX -std=c++11 -o quick_exit quick_exit.cpp
$CXX -std=c++11 -o long_startup long_startup.cpp
cd ..

# Create release directory
mkdir -p release/$TARGET
cp publish/$TARGET/Taskmaster release/$TARGET/
cp -r tests/ release/$TARGET/
cp *.config.json release/$TARGET/ 2>/dev/null || true
cp subject.md release/$TARGET/ 2>/dev/null || true
cp README.md release/$TARGET/ 2>/dev/null || true

# Set executable permissions
chmod +x release/$TARGET/Taskmaster
chmod +x release/$TARGET/tests/simple_output
chmod +x release/$TARGET/tests/heavy_output
chmod +x release/$TARGET/tests/failing_process
chmod +x release/$TARGET/tests/signal_handler
chmod +x release/$TARGET/tests/env_test
chmod +x release/$TARGET/tests/quick_exit
chmod +x release/$TARGET/tests/long_startup

echo "✅ macOS $TARGET build completed!"
echo "📁 Release files in: release/$TARGET/"
echo "🚀 Run with: ./release/$TARGET/Taskmaster"