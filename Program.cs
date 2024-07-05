
using Taskmaster.Modals;
using Taskmaster;
using Taskmaster.Enums;

static void ApplicationStart()
{
    Config config = new Config("task.config.json");

    List<Container> containers = new List<Container>
    {
        new Container
        {
            Name = "Container 1",
            Command = "echo 'Hello World!'",
            NumberOfProcesses = 1,
            StartAtLaunch = true,
            RestartPolicy = RestartPolicy.Always,
            ExpectedExitCodes = new List<int> { 0 },
            ExpectedExitCode = 0,
            StartTimeout = 1000 * 5,
            MaxRestartAttempts = 3,
            StopSignal = 15,
            KillTimeout = 1000 * 5,
            LogOutput = false,
            EnvironmentVariables = new Dictionary<string, string>(),
            WorkingDirectory = "",
            Umask = 0
        },
        new Container
        {
             Name = "Container 2",
            Command = "echo 'Hello World!'",
            NumberOfProcesses = 1,
            StartAtLaunch = true,
            RestartPolicy = RestartPolicy.Always,
            ExpectedExitCodes = new List<int> { 0 },
            ExpectedExitCode = 0,
            StartTimeout = 1000 * 5,
            MaxRestartAttempts = 3,
            StopSignal = 15,
            KillTimeout = 1000 * 5,
            LogOutput = false,
            EnvironmentVariables = new Dictionary<string, string>(),
            WorkingDirectory = "",
            Umask = 0
        }
    };

    config.Write(containers);
 

}

ApplicationStart();