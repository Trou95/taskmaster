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
                    Task.Run(() => ProcessHandler(process));
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
        if (!p)
        {
            if (process.HasExited)
                Console.WriteLine($"ProcessHandler: Process Exited{process.ExitCode}");
        }

        var container = _containers.Find(c => c.processes.Contains(process));
        if (container == null)
            return;

        StreamReader reader = process.StandardOutput;
        container.StdOut += reader.ReadToEndAsync().Result;
        //Console.WriteLine("ProcessHandler:  " + container.StdOut);

        process.WaitForExit();
        container.TaskStatus.Remove(process);

    }

    private List<Process> InitProcess(ExtendedContainer container)
    {      

        for(int i = 0; i < container.NumberOfProcesses; i++)
        {
            var process = new Process();
            process.StartInfo.FileName = container.BinaryPath;
            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) => OnProcessExit((Process?)sender, e);
            process.StartInfo.RedirectStandardOutput = true;


            container.TaskStatus.Add(process, false);
            container.processes.Add(process);

        }
        return container.processes;
    }


    public int Count { get => _containers.Count; }

    private List<ExtendedContainer> _containers { get; set; }
    private readonly IMapper _mapper;

    private class ExtendedContainer : Container
    {
        public List<Process> processes { get; set; } = new List<Process>();

        public Dictionary<Process, bool> TaskStatus { get; set; } = new Dictionary<Process, bool>();

        public string StdOut { get; set; } = "";
        public string StdErr { get; set; } = "";

    }

    /* Events */
    private void OnProcessExit(Process? process, EventArgs e)
    {

        if (process == null)
            return;

        var container = _containers.Find(c => c.processes.Contains(process));
        if (container == null)
            return;

        Console.WriteLine($"Process Exit: {container.Name} - {process.ExitCode}");

        while(container.TaskStatus.ContainsKey(process))
        {
            Thread.Sleep(1000);
        }

        container.processes.Remove(process);
        if(container.processes.Count == 0)
        {
            container.ContainerStatus = false;

            //OnContainerStop(container);
            
            try
            {
                File.WriteAllTextAsync($"{container.Name}.log", container.StdOut);
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
