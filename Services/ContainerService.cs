using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AutoMapper;
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
                
                Console.WriteLine($"Starting Container: {container.Name} - {container.BinaryPath} ProcessCount: {container.NumberOfProcesses}");

                container.ContainerStatus = ContainerStatus.Running;
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

        //OnContainerStop(container);
        try
        {
            File.WriteAllText($"./{container.Name}.log", c.StdOut);
        }
        catch
        {

        }
        finally
        {
            c.StdOut = "";
            c.TotalProcessRetryCount = 0;
            c.ContainerStatus = ContainerStatus.Waiting;
            Console.WriteLine($"Container: {container.Name} is stopped");
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
            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) => ProcessExitHandler((Process?)sender, e);
            process.StartInfo.RedirectStandardOutput = true;

            container.Processes.Add(process, new Task(() => ProcessHandler(process), container.CancellationTokenSource.Token));

        }
        return container.Processes;
    }


    public int Count { get => _containers.Count; }

    private List<ExtendedContainer> _containers { get; set; }
    private readonly IMapper _mapper;

    private class ExtendedContainer : Container
    {
        public Dictionary<Process, Task> Processes { get; set; } = new Dictionary<Process, Task>();
        public CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();
        public CancellationToken CancellationToken { get; set; }
        public Dictionary<Process, DateTime> ProcessStartTimes { get; set; } = new Dictionary<Process, DateTime>();
        public string StdOut { get; set; } = "";
        public string StdErr { get; set; } = "";
        public int TotalProcessRetryCount { get; set; } = 0;

    }

    /* Events */
    private void ProcessExitHandler(Process? process, EventArgs e)
    {
 
        if (process == null)
            return;

        var container = _containers.Find(c => c.Processes.ContainsKey(process));
        if (container == null)
            return;


        StreamReader reader = process.StandardOutput;
        container.StdOut += reader.ReadToEnd();
  

        process.WaitForExit();

        var processTotalRunTime = (process.ExitTime - container.ProcessStartTimes[process]).TotalSeconds;

        if(processTotalRunTime > container.ExpectedRunTime)
        {
            Console.WriteLine($"Process: {process.Id} Succesfully executed. Exceeded Expected Run Time: {processTotalRunTime}");
        }

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


        //Console.WriteLine($" --- Process: {process.Id} Exited ---");
        //Console.WriteLine($"StdOut: {container.StdOut}");
        //Console.WriteLine(" --- Process: Exited ---");

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

       /*
       if(container.TotalProcessRetryCount < container.MaxRestartAttempts)
        {
            Console.WriteLine($"Process Restarting");
            container.CancellationTokenSource.Cancel();
            Task.Delay(1000).Wait();
            Console.WriteLine("asdsa");
            container.TotalProcessRetryCount++;
            container.CancellationTokenSource = new CancellationTokenSource();
            Console.WriteLine("asdsa");
            container.Processes[process] = new Task(() => ProcessHandler(process), container.CancellationTokenSource.Token);
            container.Processes[process].Start();
        }
        else
        {
            StopContainer(container);
        }
       */
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
