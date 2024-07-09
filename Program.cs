
using Taskmaster.Modals;
using Taskmaster;
using Taskmaster.Enums;
using Taskmaster.Services;
using System.Runtime.InteropServices;
using Mono.Unix;
using Mono.Unix.Native;
using static Taskmaster.Services.ContainerService;
using System.Diagnostics;
using Taskmaster.Logger;



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

        app.inputService.OnCommandTyped += Events.OnCommandTyped;
        app.containerService.OnContainerStarting += Events.OnContainerStarting;
        app.containerService.OnContainerSuccessfullyStart += Events.OnContainerSuccessfullyStart;
        app.containerService.OnContainerStop += Events.OnContainerStop;
        app.containerService.OnProcessExit += Events.OnProcessExit;
        app.containerService.OnContainerRestart += Events.OnContainerRestart;
        app.OnSignalRecievedEvent += Events.OnSignalRecieved;

    }
    catch (Exception e)
    {
        Console.WriteLine($"Error: {e.Message}");
        System.Environment.Exit(1);
    }

    LogService.Add(new FileLogger("taskmaster.log"));
    LogService.Add(new DatabaseLogger());
    while (true)
    {
        app!.inputService.GetInput("> ");
    }
  
}

ApplicationStart();
