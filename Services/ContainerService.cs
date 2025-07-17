using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using AutoMapper;
using Mono.Unix.Native;
using Taskmaster.Enums;
using Taskmaster.Modals;

namespace Taskmaster.Services;

public class ContainerService
{
    public ContainerService() 
    {
        _containers = new List<ExtendedContainer>();
        _mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Container, ExtendedContainer>();
        }).CreateMapper();

        Task.Run(ProcessWatcher);
    }

    public ContainerService(List<Container> containers) : this()
    {
        _containers = _mapper.Map<List<ExtendedContainer>>(containers);
    }

    public void StartContainer(Command command)
    {
        try
        {
            Container? res = FindByCommand(command);
            if (res != null)
            {
                if (res.NumberOfProcesses < 1)
                    return;

                if (res.ContainerStatus != ContainerStatus.Waiting)
                    throw new ContainerServiceException($"Error: {res.Name} is already running");

                ExtendedContainer container = (ExtendedContainer)res;
                Console.WriteLine(container.Name + " is starting...");
                container.Processes = InitProcess(container);
                
                OnContainerStarting?.Invoke(container);

                container.ContainerStatus = ContainerStatus.Running;
              
            
                if(container.Umask != null)
                    SetUmask(Convert.ToInt32(container.Umask, 8));

                foreach (var process in container.Processes)
                { 
                    process.Value.Start();
                }            
            }
        }
        catch (ContainerServiceException e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public void StartContainerByName(string name)
    {
        Container? res = _containers.Find(c => String.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        if (res != null)
        {
            Console.WriteLine($"Starting container {name}");
            StartContainer(res.Command);
        }
        else
        {
            Console.WriteLine($"Error: Container {name} not found");
        }
    }

    public void StopContainerByName(string name)
    {
        Container? res = _containers.Find(c => String.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        if (res != null)
        {
            Console.WriteLine($"Stopping container {name}");
            StopContainer(res);
        }
        else
        {
            Console.WriteLine($"Error: Container {name} not found");
        }
    }

    public void RestartContainerByName(string name)
    {
        Container? res = _containers.Find(c => String.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        if (res != null)
        {
            Console.WriteLine($"Restarting container {name}");
            StopContainer(res);
            Task.Delay(1000).Wait();
            StartContainer(res.Command);
        }
        else
        {
            Console.WriteLine($"Error: Container {name} not found");
        }
    }


    public void StopContainer(Container container)
    {
        if (container.ContainerStatus == ContainerStatus.Waiting)
            return;

        var c = (ExtendedContainer)container;

        try
        {
            if (c.LogOutput)
            {
                Console.WriteLine($"Stopping container {c.Name} and writing logs to {c.StdOutPath} and {c.StdErrPath}");
                File.WriteAllText(c.StdOutPath, c.StdOut);
                File.WriteAllText(c.StdErrPath, c.StdErr);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            Parallel.ForEach(c.Processes, process =>
            {
                process.Key.Kill();
            });
            c.Processes.Clear();
            c.ProcessStartTimes.Clear();
            c.TotalProcessRetryCount = 0;
            if (c.LogOutput)
            {
                c.StdOut = "";
                c.StdErr = "";
            }
            c.ContainerStatus = ContainerStatus.Waiting;
            OnContainerStop?.Invoke(container);
        }
    }

    public void RestartContainer(Process process)
    {
        var container = _containers.Find(c => c.Processes.ContainsKey(process));
        if (container == null)
            return;

        if (container.TotalProcessRetryCount < container.MaxRestartAttempts)
        {
            OnContainerRestart?.Invoke(container, process);
            container.ContainerStatus = ContainerStatus.Restarting;
            container.CancellationTokenSource.Cancel();
            Task.Delay(1000).Wait();
            container.TotalProcessRetryCount++;
            container.CancellationTokenSource = new CancellationTokenSource();
            container.ProcessStartTimes.Remove(process);
            container.Processes[process] = new Task(() => ProcessHandler(process), container.CancellationTokenSource.Token);
            container.Processes[process].Start();
        }
        else
        {
            StopContainer(container);
        }
    }

    public void Add(Container container)
    {
        _containers.Add(_mapper.Map<ExtendedContainer>(container));
    }

    public void UpdateContainers(List<Container> newContainers)
    {
        var existingContainers = _containers.ToList();
        var containersToRemove = new List<ExtendedContainer>();
        var containersToAdd = new List<Container>();

        foreach (var existing in existingContainers)
        {
            var found = newContainers.FirstOrDefault(nc => nc.Name == existing.Name);
            if (found == null)
            {
                StopContainer(existing);
                containersToRemove.Add(existing);
            }
            else if (!AreContainersEqual(existing, found))
            {
                StopContainer(existing);
                containersToRemove.Add(existing);
                containersToAdd.Add(found);
            }
        }

        foreach (var newContainer in newContainers)
        {
            if (!existingContainers.Any(ec => ec.Name == newContainer.Name))
            {
                containersToAdd.Add(newContainer);
            }
        }

        foreach (var container in containersToRemove)
        {
            _containers.Remove(container);
        }

        foreach (var container in containersToAdd)
        {
            Add(container);
        }
    }

    private bool AreContainersEqual(Container existing, Container newContainer)
    {
        return existing.Name == newContainer.Name &&
               existing.Command == newContainer.Command &&
               existing.BinaryPath == newContainer.BinaryPath &&
               existing.NumberOfProcesses == newContainer.NumberOfProcesses &&
               existing.RestartPolicy == newContainer.RestartPolicy &&
               existing.MaxRestartAttempts == newContainer.MaxRestartAttempts &&
               existing.StopSignal == newContainer.StopSignal &&
               existing.KillTimeout == newContainer.KillTimeout &&
               existing.WorkingDirectory == newContainer.WorkingDirectory &&
               existing.Umask == newContainer.Umask &&
               existing.LogOutput == newContainer.LogOutput &&
               existing.ExpectedRunTime == newContainer.ExpectedRunTime &&
               existing.StartAtLaunch == newContainer.StartAtLaunch &&
               existing.ExpectedExitCodes.SequenceEqual(newContainer.ExpectedExitCodes) &&
               existing.EnvironmentVariables.SequenceEqual(newContainer.EnvironmentVariables);
    }

    public Container? FindByCommand(Command command)
    {
        return _containers.Find(c => c.Command == command);
    }

    public void SignalHandler(Signum signum)
    {
        _containers.ForEach(c =>
        {
            if (c.ContainerStatus == ContainerStatus.Running)
            {
                if (c.StopSignal > 0 && c.StopSignal == (int)signum)
                {
                    StopContainer(c);
                }
            }
        });
    }

    private void ProcessHandler(Process process)
    {
 
        bool p;
        try
        {
            p = process.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            ProcessLaunchErrorHandler(process, e.Message);
            return;
        }

        var container = _containers.Find(c => c.Processes.ContainsKey(process));
        if (container == null)
            return;
        container.ProcessStartTimes[process] = DateTime.Now;
        container.ContainerStatus = ContainerStatus.Running;
        
        if(container.LogOutput)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
    }

    private Dictionary<Process, Task> InitProcess(ExtendedContainer container)
    {      

        for(int i = 0; i < container.NumberOfProcesses; i++)
        {
            var process = new Process();
            process.StartInfo.FileName = container.BinaryPath;
            process.StartInfo.WorkingDirectory = container.WorkingDirectory;
            container.EnvironmentVariables.ToList().ForEach(e =>
            {
                process.StartInfo.Environment.Add(e.Key, e.Value);
            });

            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) => ProcessExitHandler((Process?)sender, e);

            if(container.LogOutput)
            {
                Console.WriteLine($"Logging output for container {container.Name}");
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                
                process.OutputDataReceived += (sender, e) => 
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        container.StdOut += e.Data + Environment.NewLine;
                        if (!string.IsNullOrEmpty(container.StdOutPath))
                        {
                            File.AppendAllText(container.StdOutPath, e.Data + Environment.NewLine);
                        }
                    }
                };
                
                process.ErrorDataReceived += (sender, e) => 
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        container.StdErr += e.Data + Environment.NewLine;
                        if (!string.IsNullOrEmpty(container.StdErrPath))
                        {
                            File.AppendAllText(container.StdErrPath, e.Data + Environment.NewLine);
                        }
                    }
                };
            }

            container.Processes.Add(process, new Task(() => ProcessHandler(process), container.CancellationTokenSource.Token));

        }
        return container.Processes;
    }

    [DllImport("libc", SetLastError = true)]
    private static extern uint umask(uint mask);

    private void SetUmask(int mask)
    {
        umask((uint)mask);
    }

    public int Count { get => _containers.Count; }

    private List<ExtendedContainer> _containers { get; set; }
    private readonly IMapper _mapper;

    private async void ProcessWatcher()
    {
        while(true)
        {
            Parallel.ForEach(_containers, container =>
            {
                if (container.ProcessExitTime != null)
                {
                    var time = (DateTime.Now - (DateTime)container.ProcessExitTime).TotalSeconds;
                    if (time > container.KillTimeout)
                    {
                        Parallel.ForEach(container.Processes, process =>
                        {
                            process.Key.Kill();
                        });
                    }
                }
            });
            await Task.Delay(1000);
        }
    }

    public void PrintContainers()
    {
        _containers.ForEach(c =>
        {
            Console.WriteLine($"{c} Status: {c.ContainerStatus}");
        });
    }

    private class ExtendedContainer : Container
    {
        public Dictionary<Process, Task> Processes { get; set; } = new Dictionary<Process, Task>();
        public CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();
        public CancellationToken CancellationToken { get; set; }
        public Dictionary<Process, DateTime> ProcessStartTimes { get; set; } = new Dictionary<Process, DateTime>();
        public DateTime? ProcessExitTime { get; set; } = null;
        public string StdOut { get; set; } = "";
        public string StdErr { get; set; } = "";
        public int TotalProcessRetryCount { get; set; } = 0;

    }

    /* Events */

    public delegate void OnContainerStopHandler(Container container);
    public event OnContainerStopHandler? OnContainerStop;

    public delegate void OnContainerStartingHandler(Container container);
    public event OnContainerStartingHandler? OnContainerStarting;

    public delegate void OnContainerSuccessfullyStartHandler(Container container, int RunTime);
    public event OnContainerSuccessfullyStartHandler? OnContainerSuccessfullyStart;

    public delegate void OnContainerRestartHandler(Container container, Process process);
    public event OnContainerRestartHandler? OnContainerRestart;

    private void ProcessExitHandler(Process? process, EventArgs e)
    {
 
        if (process == null)
            return;

        var container = _containers.Find(c => c.Processes.ContainsKey(process));
        if (container == null)
            return;


        if(container.LogOutput)
        {
            // Asynchronous output reading kullandığımız için synchronous okuma yapamayız
            // Output'lar zaten event handler'larda toplanıyor
        }

        container.ProcessExitTime = DateTime.Now;
        process.WaitForExit();

        var processTotalRunTime = container.ProcessStartTimes.ContainsKey(process) ? (process.ExitTime - container.ProcessStartTimes[process]).TotalSeconds : 0;
        if(container.ExpectedRunTime != 0 && processTotalRunTime >= container.ExpectedRunTime)
            OnContainerSuccessfullyStart?.Invoke(container, (int)processTotalRunTime);
        
        if(container.RestartPolicy != RestartPolicy.Never)
        {
            if(container.RestartPolicy == RestartPolicy.Always)
            {
                LogService.Log($"Container {container.Name} exited with code {process.ExitCode}, restarting (always policy)");
                RestartContainer(process);
                return;
            }
            if(container.RestartPolicy == RestartPolicy.OnFailure)
            {
                if(!container.ExpectedExitCodes.Contains(process.ExitCode))
                {
                    LogService.Log($"Container {container.Name} died unexpectedly with exit code {process.ExitCode}, restarting");
                    RestartContainer(process);
                    return;
                }                
            }
        }
        else
        {
            if(!container.ExpectedExitCodes.Contains(process.ExitCode))
            {
                LogService.Log($"Container {container.Name} died unexpectedly with exit code {process.ExitCode}");
            }
        }

        container.Processes.Remove(process);
        container.ProcessStartTimes.Remove(process);
        if(container.Processes.Count == 0)
        {
           StopContainer(container);
        }
    }

    private void ProcessLaunchErrorHandler(Process process, string message)
    {
       var container = _containers.Find(c => c.Processes.ContainsKey(process));
       if(container == null)
            return;

       RestartContainer(process);
    }

    /* Exceptions */
    private class ContainerServiceException : Exception 
    { 
        public ContainerServiceException(string message) : base(message) { }
    }

    private class ContainerNotFoundException : ContainerServiceException 
    { 
        public ContainerNotFoundException(string message) : base(message) { }
    }
}
