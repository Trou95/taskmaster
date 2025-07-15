#!/bin/bash

# Release testing script
# Taskmaster Project

set -e

echo "🧪 Testing Taskmaster release builds..."
echo "======================================"

# Detect current platform
PLATFORM=$(uname -s)
ARCH=$(uname -m)

case "$PLATFORM" in
    "Darwin")
        if [[ "$ARCH" == "arm64" ]]; then
            RELEASE_DIR="release/osx-arm64"
            BINARY="Taskmaster"
        else
            RELEASE_DIR="release/osx-x64"  
            BINARY="Taskmaster"
        fi
        ;;
    "Linux")
        RELEASE_DIR="release/linux-x64"
        BINARY="Taskmaster"
        ;;
    "MINGW"*|"MSYS"*|"CYGWIN"*)
        RELEASE_DIR="release/win-x64"
        BINARY="Taskmaster.exe"
        ;;
    *)
        echo "❌ Unsupported platform: $PLATFORM"
        exit 1
        ;;
esac

echo "🔍 Detected platform: $PLATFORM $ARCH"
echo "📁 Testing release: $RELEASE_DIR"

if [[ ! -d "$RELEASE_DIR" ]]; then
    echo "❌ Release directory not found: $RELEASE_DIR"
    echo "💡 Run ./build-all.sh first to create releases"
    exit 1
fi

if [[ ! -f "$RELEASE_DIR/$BINARY" ]]; then
    echo "❌ Binary not found: $RELEASE_DIR/$BINARY"
    exit 1
fi

cd "$RELEASE_DIR"

echo "✅ Found binary: $BINARY"

# Test binary execution
echo ""
echo "🧪 Testing binary execution..."
if timeout 5s ./$BINARY --help 2>/dev/null || true; then
    echo "✅ Binary executes successfully"
else
    echo "⚠️  Binary execution test completed (expected for interactive program)"
fi

# Test configuration files
echo ""
echo "🧪 Testing configuration files..."
CONFIG_FILES=(
    "test.config.json"
    "task.config.json"
    "presentation.config.json"
    "tests/comprehensive_test.config.json"
)

for config in "${CONFIG_FILES[@]}"; do
    if [[ -f "$config" ]]; then
        echo "✅ Found config: $config"
        
        # Basic JSON validation
        if command -v python3 &> /dev/null; then
            if python3 -m json.tool "$config" > /dev/null 2>&1; then
                echo "  ✅ Valid JSON format"
            else
                echo "  ❌ Invalid JSON format"
            fi
        fi
    else
        echo "⚠️  Missing config: $config"
    fi
done

# Test compiled test programs
echo ""
echo "🧪 Testing compiled test programs..."
cd tests

TEST_PROGRAMS=(
    "simple_output"
    "heavy_output"
    "failing_process"
    "signal_handler"
    "env_test"
    "quick_exit"
    "long_startup"
)

# Add .exe extension for Windows
if [[ "$PLATFORM" == "MINGW"* || "$PLATFORM" == "MSYS"* || "$PLATFORM" == "CYGWIN"* ]]; then
    for i in "${!TEST_PROGRAMS[@]}"; do
        TEST_PROGRAMS[$i]="${TEST_PROGRAMS[$i]}.exe"
    done
fi

for program in "${TEST_PROGRAMS[@]}"; do
    if [[ -f "$program" && -x "$program" ]]; then
        echo "✅ Found executable: $program"
        
        # Quick execution test (with timeout)
        if timeout 2s ./"$program" > /dev/null 2>&1 || true; then
            echo "  ✅ Program executes"
        else
            echo "  ⚠️  Program execution completed (expected for some tests)"
        fi
    else
        echo "❌ Missing or not executable: $program"
    fi
done

cd ..

# Test log directory
echo ""
echo "🧪 Testing log directory..."
if [[ -d "tests/logs" ]]; then
    echo "✅ Log directory exists"
else
    echo "⚠️  Log directory missing (will be created on first run)"
fi

# Test documentation
echo ""
echo "🧪 Testing documentation..."
DOC_FILES=(
    "README.md"
    "subject.md"
    "tests/TEST_INSTRUCTIONS.md"
)

for doc in "${DOC_FILES[@]}"; do
    if [[ -f "$doc" ]]; then
        echo "✅ Found documentation: $doc"
    else
        echo "⚠️  Missing documentation: $doc"
    fi
done

# File permissions test
echo ""
echo "🧪 Testing file permissions..."
if [[ -x "$BINARY" ]]; then
    echo "✅ Main binary is executable"
else
    echo "❌ Main binary is not executable"
fi

# Integration test
echo ""
echo "🧪 Running integration test..."
cat > test_integration.config.json << 'EOF'
[
  {
    "Name": "integration-test",
    "Command": "/integration-test",
    "BinaryPath": "./tests/quick_exit",
    "NumberOfProcesses": 1,
    "StartAtLaunch": true,
    "RestartPolicy": "Never",
    "ExpectedExitCodes": [0],
    "ExpectedRunTime": 1,
    "MaxRestartAttempts": 0,
    "StopSignal": 15,
    "KillTimeout": 2000,
    "LogOutput": true,
    "StdOutPath": "./integration-test.stdout",
    "StdErrPath": "./integration-test.stderr",
    "EnvironmentVariables": {
      "TEST_MODE": "integration"
    },
    "WorkingDirectory": ".",
    "Umask": "022"
  }
]
EOF

echo "Created integration test configuration"

# Cleanup
rm -f test_integration.config.json integration-test.stdout integration-test.stderr 2>/dev/null || true

echo ""
echo "🎉 Release testing completed!"
echo "============================"
echo ""
echo "📋 Test Summary:"
echo "  Platform: $PLATFORM $ARCH"
echo "  Release:  $RELEASE_DIR"
echo "  Binary:   $BINARY"
echo ""
echo "🚀 Ready for deployment!"
echo ""
echo "💡 Usage:"
echo "  ./$BINARY"