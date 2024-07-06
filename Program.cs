
using Taskmaster.Modals;
using Taskmaster;
using Taskmaster.Enums;
using Taskmaster.Services;

static void ApplicationStart()
{
    App app = null;
    //app.inputService.OnCommandTyped += OnCommandTyped;

    try
    {
        Console.WriteLine("asdad");
        app = new App();

        var containers = app.config.Read();
        foreach (var container in containers)
        {
            app.commandService.Add(container.Command);
        }
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