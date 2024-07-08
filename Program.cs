
using Taskmaster.Modals;
using Taskmaster;
using Taskmaster.Enums;
using Taskmaster.Services;
using System.Runtime.InteropServices;
using Mono.Unix;
using Mono.Unix.Native;
using static Taskmaster.Services.ContainerService;
using System.Diagnostics;


static void ApplicationStart()
{
    App? app = null;

    try
    {
        app = new App();

        var res = app.config.Read();
 
        if(res != null)
        {
            foreach (var c in res)
            {
                app.commandService.Add(c.Command);
                app.containerService.Add(c);
            }
        } 

        app.inputService.OnCommandTyped += OnCommandTyped;
        app.containerService.OnContainerStarting += OnContainerStarting;
        app.containerService.OnContainerSuccessfullyStart += OnContainerSuccessfullyStart;
        app.containerService.OnContainerStop += OnContainerStop;
        app.containerService.OnProcessExit += OnProcessExit;
        app.OnSignalRecievedEvent += OnSignalRecieved;

    }
    catch (Exception e)
    {
        Console.WriteLine($"Error: {e.Message}");
        System.Environment.Exit(1);
    }

    while (true)
    {
        app!.inputService.GetInput("> ");
    }
  
}

ApplicationStart();

static void OnCommandTyped(IApp app, string command)
{ 
    if(app.commandService.IsCommandExist(command))
    {
        app.containerService.StartContainer(command);
    }
    else
    {
        //Console.WriteLine($"Command: {command} is not exist");
    }
}

static void OnContainerStarting(Container container)
{
    //Console.WriteLine($"Container: {container.Name} is starting");
}

static void OnContainerSuccessfullyStart(Container container, int RunTime)
{
    //Console.WriteLine($"Container: {container.Name} is successfully started");
}

static void OnContainerStop(Container container)
{
    //Console.WriteLine($"Container: {container.Name} is stopped");
}

static void OnProcessExit(Process process)
{
   //Console.WriteLine($"Process: {process.Id} is exited");
}

static void OnSignalRecieved(IApp app, Signum signal)
{
    //app.containerService.SignalHandler(signal);
}