
using Taskmaster.Modals;
using Taskmaster;
using Taskmaster.Enums;
using Taskmaster.Services;

static void ApplicationStart()
{
    App? app = null;

    try
    {
        app = new App();

        var containers = app.config.Read();
 
        if(containers != null)
        {
            foreach (var container in containers)
                app.commandService.Add(container.Command);
        } 

        app.inputService.OnCommandTyped += OnCommandTyped;
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
    //Console.WriteLine($"Command: {command}");
    if(app.commandService.IsCommandExist(command))
    {
        Console.WriteLine($"Command: {command} is exist");
    }
    else
    {
        Console.WriteLine($"Command: {command} is not exist");
    }
}