# Taskmaster Test Suite

## Test Programs Created

### 1. `simple_output.cpp` 
- **Purpose**: Basic output testing
- **Behavior**: Outputs numbered messages every 2 seconds
- **Test Cases**: Basic logging, restart on failure

### 2. `heavy_output.cpp`
- **Purpose**: Stress test output handling
- **Behavior**: Generates large amounts of stdout/stderr rapidly
- **Test Cases**: Heavy logging, buffer handling

### 3. `failing_process.cpp`
- **Purpose**: Test restart policies
- **Behavior**: Randomly fails with different exit codes
- **Test Cases**: Restart on failure, max retry limits

### 4. `signal_handler.cpp`
- **Purpose**: Test graceful shutdown
- **Behavior**: Handles SIGTERM, SIGINT, SIGUSR1/2 gracefully
- **Test Cases**: Signal handling, graceful stop

### 5. `env_test.cpp`
- **Purpose**: Environment variable validation
- **Behavior**: Prints all environment variables and exits
- **Test Cases**: Environment variable passing

### 6. `quick_exit.cpp`
- **Purpose**: Test restart-always policy
- **Behavior**: Exits after 5 seconds
- **Test Cases**: Always restart policy

### 7. `long_startup.cpp`
- **Purpose**: Test startup time requirements
- **Behavior**: Takes 10 seconds to fully start
- **Test Cases**: ExpectedRunTime validation

## Configuration File: `comprehensive_test.config.json`

### Test Scenarios Covered:

1. **Basic Operation** (`simple-output`)
   - AutoStart: Yes
   - RestartPolicy: OnFailure
   - Basic logging

2. **Heavy Output** (`heavy-output`)
   - AutoStart: No (manual start)
   - High volume stdout/stderr
   - Tests buffer handling

3. **Failure Handling** (`failing-process`)
   - AutoStart: No
   - RestartPolicy: OnFailure
   - MaxRestartAttempts: 5
   - Tests retry logic

4. **Signal Handling** (`signal-handler`)
   - AutoStart: No
   - RestartPolicy: Never
   - StopSignal: 15 (SIGTERM)
   - KillTimeout: 10 seconds

5. **Environment Variables** (`env-test`)
   - AutoStart: No
   - Comprehensive env var testing
   - 15 different environment variables

6. **Always Restart** (`quick-exit`)
   - AutoStart: No
   - RestartPolicy: Always
   - Quick exit for continuous restart testing

7. **Slow Startup** (`long-startup`)
   - AutoStart: No
   - ExpectedRunTime: 12 seconds
   - Tests startup time validation

8. **Multiple Instances** (`multi-instance`)
   - AutoStart: No
   - NumberOfProcesses: 3
   - Tests multi-process management

## How to Use

1. **Compile all programs**:
   ```bash
   cd tests
   g++ -o simple_output simple_output.cpp
   g++ -o heavy_output heavy_output.cpp
   g++ -o failing_process failing_process.cpp
   g++ -o signal_handler signal_handler.cpp
   g++ -o env_test env_test.cpp
   g++ -o quick_exit quick_exit.cpp
   g++ -o long_startup long_startup.cpp
   ```

2. **Update main config** to use test config:
   ```bash
   cp tests/comprehensive_test.config.json test.config.json
   ```

3. **Run Taskmaster**:
   ```bash
   dotnet run
   ```

4. **Test Commands**:
   ```
   /status                    # See all program status
   /start heavy-output        # Start heavy output test
   /start failing-process     # Test failure restart
   /start signal-handler      # Test signal handling
   /start env-test           # Test environment variables
   /start quick-exit         # Test always restart
   /start long-startup       # Test slow startup
   /start multi-instance     # Test multiple processes
   /stop [name]              # Stop any service
   /restart [name]           # Restart any service
   ```

## Expected Results

- **simple-output**: Should start automatically and run continuously
- **heavy-output**: Should handle large amounts of output without issues
- **failing-process**: Should restart up to 5 times then stop
- **signal-handler**: Should exit gracefully when stopped
- **env-test**: Should show all environment variables correctly
- **quick-exit**: Should restart continuously (Always policy)
- **long-startup**: Should be marked as successfully started after 12 seconds
- **multi-instance**: Should run 3 separate instances

## Log Files

All output will be logged to `./tests/logs/[service-name].stdout/stderr`

## Subject.md Requirements Coverage

✅ **Process Management**: All restart policies (Always/OnFailure/Never)  
✅ **Output Handling**: Heavy output testing, stdout/stderr separation  
✅ **Signal Handling**: Graceful shutdown with configurable timeouts  
✅ **Environment Variables**: Comprehensive environment testing  
✅ **Multiple Processes**: Multi-instance testing  
✅ **Startup Time**: ExpectedRunTime validation  
✅ **Configuration**: All config parameters tested  
✅ **Edge Cases**: Failing processes, slow startup, heavy output  