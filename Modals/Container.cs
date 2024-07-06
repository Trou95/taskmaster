
using System.Diagnostics;
using System.Text.Json.Serialization;
using Taskmaster.Enums;


namespace Taskmaster.Modals;

public class Container
{
    public required string Name { get; set; }
    public required string Command { get; set; }
    public required string BinaryPath { get; set; }
    public bool ContainerStatus { get; set; }
    public int NumberOfProcesses { get; set; }
    public bool StartAtLaunch { get; set; }
    public RestartPolicy RestartPolicy { get; set; }
    public List<int> ExpectedExitCodes { get; set; }
    public int ExpectedExitCode { get; set; }
    public uint StartTimeout { get; set; }
    public int MaxRestartAttempts { get; set; }
    public int StopSignal { get; set; }
    public uint KillTimeout { get; set; }
    public bool LogOutput { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; set; }
    public string WorkingDirectory { get; set; }
    public int Umask { get; set; }

    public Container()
    {
        ExpectedExitCodes = new List<int>();
        EnvironmentVariables = new Dictionary<string, string>();

        ContainerStatus = false;
        NumberOfProcesses = 1;
        StartAtLaunch = false;
        RestartPolicy = RestartPolicy.Never;
        ExpectedExitCode = 0;
        StartTimeout = 1000 * 5;
        MaxRestartAttempts = 3;
        StopSignal = 15;
        KillTimeout = 1000 * 5;
        LogOutput = false;
        WorkingDirectory = "";
        Umask = 0;
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
       return $"Name: {Name}, Command: {Command}, NumberOfProcesses: {NumberOfProcesses}, StartAtLaunch: {StartAtLaunch}, RestartPolicy: {RestartPolicy}, ExpectedExitCodes: {ExpectedExitCodes}, ExpectedExitCode: {ExpectedExitCode}, StartTimeout: {StartTimeout}, MaxRestartAttempts: {MaxRestartAttempts}, StopSignal: {StopSignal}, KillTimeout: {KillTimeout}, LogOutput: {LogOutput}, EnvironmentVariables: {EnvironmentVariables}, WorkingDirectory: {WorkingDirectory}, Umask: {Umask}";
    }
}
