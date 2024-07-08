using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AutoMapper;
using Mono.Unix.Native;
using Taskmaster.Enums;
using Taskmaster.Modals;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    public void StopContainer(Container container)
    {
        if (container.ContainerStatus == ContainerStatus.Waiting)
            return;
 
        var c = (ExtendedContainer)container;

        try
        {
            if(c.LogOutput)
            {
                File.WriteAllText(c.StdOutPath, c.StdOut);
                File.WriteAllText(c.StdErrPath, c.StdErr);
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
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
            Console.WriteLine($"Process Restarting");
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
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
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

    public delegate void OnProcessExitHandler(Process process);
    public event OnProcessExitHandler? OnProcessExit;

    public delegate void OnContainerStartingHandler(Container container);
    public event OnContainerStartingHandler? OnContainerStarting;

    public delegate void OnContainerSuccessfullyStartHandler(Container container, int RunTime);
    public event OnContainerSuccessfullyStartHandler? OnContainerSuccessfullyStart;

    private void ProcessExitHandler(Process? process, EventArgs e)
    {
 
        if (process == null)
            return;

        var container = _containers.Find(c => c.Processes.ContainsKey(process));
        if (container == null)
            return;


        if(container.LogOutput)
        {
            StreamReader readerOut = process.StandardOutput;
            StreamReader readerErr = process.StandardError;
            container.StdOut += readerOut.ReadToEnd();
            container.StdErr += readerErr.ReadToEnd();
        }

        container.ProcessExitTime = DateTime.Now;
        process.WaitForExit();

        var processTotalRunTime = (process.ExitTime - container.ProcessStartTimes[process]).TotalSeconds;
        if(container.ExpectedRunTime == 0 || processTotalRunTime > container.ExpectedRunTime)
            OnContainerSuccessfullyStart?.Invoke(container, (int)processTotalRunTime);
        
        if(container.RestartPolicy != RestartPolicy.Never)
        {
            if(container.RestartPolicy == RestartPolicy.Always)
            {
                RestartContainer(process);
                return;
            }
            if(container.RestartPolicy == RestartPolicy.OnFailure)
            {
                if(!container.ExpectedExitCodes.Contains(process.ExitCode))
                {
                    RestartContainer(process);
                    return;
                }                
            }
        }

        OnProcessExit?.Invoke(process);

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
