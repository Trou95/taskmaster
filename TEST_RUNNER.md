# Taskmaster Test Plan

## Manual Testing Instructions

### 1. **Build and Run**
```bash
cd /Users/gorkem/Desktop/taskmaster
dotnet build
dotnet run
```

### 2. **Test Basic Commands**

#### Test Help Command
```
> /help
```
**Expected**: Should show all available commands with descriptions

#### Test Status Command  
```
> /status
```
**Expected**: Should show status of all containers (TestAutostart should be running if autostart works)

### 3. **Test Container Management**

#### Test Manual Start
```
> /start TestManual
```
**Expected**: Should start the TestManual container with sleep command

#### Test Stop
```
> /stop TestManual
```
**Expected**: Should stop the TestManual container

#### Test Restart
```
> /restart TestAutostart
```
**Expected**: Should restart the TestAutostart container

### 4. **Test Configuration Reload**

#### Test Manual Reload
```
> /reloadconfig
```
**Expected**: Should reload configuration and log the event

#### Test SIGHUP Reload
In another terminal:
```bash
# Find the process ID
ps aux | grep taskmaster
# Send SIGHUP signal
kill -HUP <pid>
```
**Expected**: Should reload configuration via signal

### 5. **Test Output Redirection**
Check if output files are created:
```bash
ls -la /tmp/test-*.stdout
ls -la /tmp/test-*.stderr
cat /tmp/test-autostart.stdout
```
**Expected**: Files should exist and contain process output

### 6. **Test Logging**
Check the log file:
```bash
cat taskmaster.log
```
**Expected**: Should contain:
- Container starting events
- Container stopping events  
- Configuration reload events
- Unexpected death events (if any)

### 7. **Test Autostart Feature**
1. Stop all containers: `/stop TestAutostart`
2. Exit program: `/quit`
3. Restart program: `dotnet run`
4. Check status: `/status`

**Expected**: TestAutostart should be running automatically

### 8. **Test Exit**
```
> /quit
```
**Expected**: Should gracefully shutdown the program

## Subject Requirements Checklist

### ✅ Mandatory Features
- [ ] **Process Management**: Start/stop/restart child processes
- [ ] **Configuration File**: JSON format configuration loading
- [ ] **SIGHUP Reload**: Configuration reload via signal
- [ ] **Control Shell**: Interactive command shell
- [ ] **Logging System**: Events logged to file

### ✅ Shell Commands  
- [ ] `/status` - Show container status
- [ ] `/start <name>` - Start specific container
- [ ] `/stop <name>` - Stop specific container  
- [ ] `/restart <name>` - Restart specific container
- [ ] `/reloadconfig` - Manual configuration reload
- [ ] `/quit` - Exit program
- [ ] `/help` - Show help

### ✅ Configuration Properties
- [ ] **Command**: Program command to execute
- [ ] **NumberOfProcesses**: Number of processes to run
- [ ] **StartAtLaunch**: Auto-start at launch
- [ ] **RestartPolicy**: Always/Never/OnFailure
- [ ] **ExpectedExitCodes**: Expected exit codes
- [ ] **ExpectedRunTime**: Minimum runtime for success
- [ ] **MaxRestartAttempts**: Maximum restart attempts
- [ ] **StopSignal**: Signal for graceful stop
- [ ] **KillTimeout**: Timeout before force kill
- [ ] **LogOutput**: Enable output logging
- [ ] **StdOutPath/StdErrPath**: Output file paths
- [ ] **EnvironmentVariables**: Environment variables
- [ ] **WorkingDirectory**: Working directory
- [ ] **Umask**: File creation mask

### ✅ Advanced Features
- [ ] **Process Status Accuracy**: Real-time process monitoring
- [ ] **Unexpected Death Logging**: Log unexpected process exits
- [ ] **Graceful Shutdown**: Proper signal handling
- [ ] **Output Redirection**: Stdout/stderr to files
- [ ] **Configuration Validation**: Error handling for invalid configs

## Expected Results

After running all tests, you should see:
1. All containers behaving according to their configuration
2. Proper logging of all events in taskmaster.log
3. Output files created in /tmp/ with process output
4. Configuration reloading working both manually and via SIGHUP
5. All shell commands working correctly
6. Autostart functionality working on program startup

## Known Issues to Verify Fixed
1. ✅ `/stop` and `/restart` commands now implemented
2. ✅ `/quit` command for graceful shutdown
3. ✅ Autostart functionality now works
4. ✅ Stdout/stderr redirection to files
5. ✅ Configuration reload logging
6. ✅ Unexpected death logging
7. ✅ Starttime validation logic corrected
8. ✅ Help command and better error messages