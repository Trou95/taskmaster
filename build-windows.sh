#!/bin/bash

# Build script for Windows x64
# Taskmaster Project
# Note: This script can run on Linux/macOS to cross-compile for Windows

set -e

echo "Building Taskmaster for Windows x64..."

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf bin/Release/net8.0/win-x64/
rm -rf publish/win-x64/

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

# Build for Windows x64
echo "Building for Windows x64..."
dotnet publish -c Release -r win-x64 --self-contained true -o publish/win-x64/ \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:IncludeNativeLibrariesForSelfExtract=true

# Check if we're on Windows (can compile with MSVC/MinGW)
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" || "$OSTYPE" == "win32" ]]; then
    echo "Compiling test programs for Windows..."
    cd tests
    
    # Try different Windows C++ compilers
    if command -v cl &> /dev/null; then
        echo "Using MSVC compiler"
        cl /EHsc simple_output.cpp /Fe:simple_output.exe
        cl /EHsc heavy_output.cpp /Fe:heavy_output.exe
        cl /EHsc failing_process.cpp /Fe:failing_process.exe
        cl /EHsc signal_handler.cpp /Fe:signal_handler.exe
        cl /EHsc env_test.cpp /Fe:env_test.exe
        cl /EHsc quick_exit.cpp /Fe:quick_exit.exe
        cl /EHsc long_startup.cpp /Fe:long_startup.exe
    elif command -v g++ &> /dev/null; then
        echo "Using MinGW g++ compiler"
        g++ -static -std=c++11 -o simple_output.exe simple_output.cpp
        g++ -static -std=c++11 -o heavy_output.exe heavy_output.cpp
        g++ -static -std=c++11 -o failing_process.exe failing_process.cpp
        g++ -static -std=c++11 -o signal_handler.exe signal_handler.cpp
        g++ -static -std=c++11 -o env_test.exe env_test.cpp
        g++ -static -std=c++11 -o quick_exit.exe quick_exit.cpp
        g++ -static -std=c++11 -o long_startup.exe long_startup.cpp
    else
        echo "⚠️  Warning: No Windows C++ compiler found"
        echo "   Test programs will not be compiled"
        echo "   Install Visual Studio Build Tools or MinGW"
    fi
    cd ..
else
    echo "⚠️  Cross-compiling from non-Windows system"
    echo "   .NET binary will work, but test programs need to be compiled on Windows"
    echo "   Copy source files and compile manually on target Windows system"
fi

# Create release directory
mkdir -p release/win-x64
cp publish/win-x64/Taskmaster.exe release/win-x64/
cp -r tests/ release/win-x64/
cp *.config.json release/win-x64/ 2>/dev/null || true
cp subject.md release/win-x64/ 2>/dev/null || true
cp README.md release/win-x64/ 2>/dev/null || true

# Create a batch file for Windows users
cat > release/win-x64/build-tests.bat << 'EOF'
@echo off
echo Compiling test programs for Windows...
cd tests

if exist "C:\Program Files\Microsoft Visual Studio" (
    echo Using MSVC compiler
    cl /EHsc simple_output.cpp /Fe:simple_output.exe
    cl /EHsc heavy_output.cpp /Fe:heavy_output.exe
    cl /EHsc failing_process.cpp /Fe:failing_process.exe
    cl /EHsc signal_handler.cpp /Fe:signal_handler.exe
    cl /EHsc env_test.cpp /Fe:env_test.exe
    cl /EHsc quick_exit.cpp /Fe:quick_exit.exe
    cl /EHsc long_startup.cpp /Fe:long_startup.exe
) else if exist "C:\mingw64\bin\g++.exe" (
    echo Using MinGW g++ compiler
    C:\mingw64\bin\g++.exe -static -std=c++11 -o simple_output.exe simple_output.cpp
    C:\mingw64\bin\g++.exe -static -std=c++11 -o heavy_output.exe heavy_output.cpp
    C:\mingw64\bin\g++.exe -static -std=c++11 -o failing_process.exe failing_process.cpp
    C:\mingw64\bin\g++.exe -static -std=c++11 -o signal_handler.exe signal_handler.cpp
    C:\mingw64\bin\g++.exe -static -std=c++11 -o env_test.exe env_test.cpp
    C:\mingw64\bin\g++.exe -static -std=c++11 -o quick_exit.exe quick_exit.cpp
    C:\mingw64\bin\g++.exe -static -std=c++11 -o long_startup.exe long_startup.cpp
) else (
    echo Error: No C++ compiler found
    echo Please install Visual Studio Build Tools or MinGW
    pause
    exit /b 1
)

cd ..
echo Test programs compiled successfully!
pause
EOF

echo "✅ Windows x64 build completed!"
echo "📁 Release files in: release/win-x64/"
echo "🚀 Run with: release/win-x64/Taskmaster.exe"
if [[ "$OSTYPE" != "msys" && "$OSTYPE" != "cygwin" && "$OSTYPE" != "win32" ]]; then
    echo "📝 Note: Run build-tests.bat on Windows to compile test programs"
fi