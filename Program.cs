
using Taskmaster;
using Taskmaster.Services;
using Taskmaster.Logger;


static void ApplicationStart()
{
    LogService.Add(new FileLogger("taskmaster.log"));
    
    App? app = null;
    try
    {
        app = new App();
        var config = app.config.Read();
 
        if(config != null)
        {
            foreach (var c in config)
            {
                app.commandService.Add(new (c.Command, false));
                app.containerService.Add(c);
                
            
                
                if (c.StartAtLaunch)
                {
                    Console.WriteLine($"Path: {c.StdOutPath}");
                    app.containerService.StartContainer(c.Command);
                }
            }
        } 

        app.inputService.OnCommandTyped += Events.OnCommandTyped;
        app.containerService.OnContainerStarting += Events.OnContainerStarting;
        app.containerService.OnContainerSuccessfullyStart += Events.OnContainerSuccessfullyStart;
        app.containerService.OnContainerStop += Events.OnContainerStop;
        app.containerService.OnContainerRestart += Events.OnContainerRestart;
        app.OnSignalRecievedEvent += Events.OnSignalRecieved;
 
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error: {e.Message}");
        System.Environment.Exit(1);
    }

    LogService.Add(new FileLogger("taskmaster.log"));
    //LogService.Add(new DatabaseLogger());
    while (true)
    {
        app!.inputService.GetInput("> ");
    }
  
}

ApplicationStart();
