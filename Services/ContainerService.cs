using System.Diagnostics;
using System.Runtime.InteropServices;
using AutoMapper;
using Taskmaster.Modals;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Taskmaster.Services;

public class ContainerService
{
    public ContainerService() 
    {
        _containers = new List<ExtendedContainer>();
        _mapper = new MapperConfiguration(cfg => { 
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

                if (res.ContainerStatus)
                    throw new ContainerServiceException($"Error: {res.Name} is already running");

                ExtendedContainer container = (ExtendedContainer)res;
                container.processes = InitProcess(container); 
                
                Console.WriteLine($"Starting Container: {container.Name} - {container.BinaryPath} ProcessCount: {container.NumberOfProcesses}");

                container.ContainerStatus = true;
                foreach (var process in container.processes)
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
        var p = process.Start();
        Console.WriteLine("ProcessHander Start:  " + p);
        if (!p)
        {
            if (process.HasExited)
                Console.WriteLine($"ProcessHandler: Process Exited{process.ExitCode}");
        }

        var container = _containers.Find(c => c.processes.ContainsKey(process));
        if (container == null)
            return;
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

            container.processes.Add(process, new Task(() => ProcessHandler(process)));

        }
        return container.processes;
    }


    public int Count { get => _containers.Count; }

    private List<ExtendedContainer> _containers { get; set; }
    private readonly IMapper _mapper;

    private class ExtendedContainer : Container
    {
        //public List<Process> processes { get; set; } = new List<Process>();

        public Dictionary<Process, Task> processes { get; set; } = new Dictionary<Process, Task>();

        public string StdOut { get; set; } = "";
        public string StdErr { get; set; } = "";

    }

    /* Events */
    private void ProcessExitHandler(Process? process, EventArgs e)
    {
        Console.WriteLine("OnProcessExit");

        if (process == null)
            return;

        var container = _containers.Find(c => c.processes.ContainsKey(process));
        if (container == null)
            return;


        StreamReader reader = process.StandardOutput;
        container.StdOut += reader.ReadToEnd();
        Console.WriteLine("OnProcessOut: " + container.StdOut);

        process.WaitForExit();
        Console.WriteLine($"OnProcessExit2 : {container.Name} - {process.ExitCode}");
        
        //container.processes[process].Dispose();
        container.processes.Remove(process);
        if(container.processes.Count == 0)
        {
            container.ContainerStatus = false;

            //OnContainerStop(container);
        
            try
            {
                File.WriteAllText($"./{container.Name}.log", container.StdOut);
            }
            catch
            {

            }
            Console.WriteLine($"Container: {container.Name} is stopped");
        }
  
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
