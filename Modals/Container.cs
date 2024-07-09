
using System.Diagnostics;
using System.Text.Json.Serialization;
using Taskmaster.Enums;


namespace Taskmaster.Modals;

public class Container
{
    public required string Name { get; set; }
    public required string Command { get; set; }
    public required string BinaryPath { get; set; }
    public ContainerStatus ContainerStatus { get; set; }
    public int NumberOfProcesses { get; set; }
    public bool StartAtLaunch { get; set; }
    public RestartPolicy RestartPolicy { get; set; }
    public List<int> ExpectedExitCodes { get; set; }
    public uint ExpectedRunTime { get; set; }
    public int MaxRestartAttempts { get; set; }
    public int StopSignal { get; set; }
    public uint KillTimeout { get; set; }
    public bool LogOutput { get; set; }
    public string StdOutPath { get; set; }
    public string StdErrPath { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; set; }
    public string WorkingDirectory { get; set; }
    public string? Umask { get; set; }

    public Container()
    {
        ExpectedExitCodes = new List<int>();
        EnvironmentVariables = new Dictionary<string, string>();

        ContainerStatus = ContainerStatus.Waiting;
        NumberOfProcesses = 1;
        StartAtLaunch = false;
        RestartPolicy = RestartPolicy.Never;
        ExpectedRunTime = 0;
        MaxRestartAttempts = 3;
        StopSignal = 15;
        KillTimeout = 1000 * 5;
        LogOutput = false;
        StdOutPath = "";
        StdErrPath = "";
        WorkingDirectory = "";
        Umask = null;
    }


    /// <summary>
    /// Prints some information about the container.
    /// </summary>
    /// <returns>
    /// Container Name and Command as a string.
    /// </returns>
    public string Print()
    {
        return $"Name: {Name} - Command: {Command}";
    }

    public override string ToString()
    {
       return $"Name: {Name}, Command: {Command}, NumberOfProcesses: {NumberOfProcesses}, StartAtLaunch: {StartAtLaunch}, RestartPolicy: {RestartPolicy}, ExpectedExitCodes: {ExpectedExitCodes}, StartTimeout: {ExpectedRunTime}, MaxRestartAttempts: {MaxRestartAttempts}, StopSignal: {StopSignal}, KillTimeout: {KillTimeout} EnvironmentVariables: {EnvironmentVariables}, WorkingDirectory: {WorkingDirectory}, Umask: {Umask}";
    }
}
