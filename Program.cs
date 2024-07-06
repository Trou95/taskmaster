
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
        app.containerService.StartContainer(command);
    }
    else
    {
        Console.WriteLine($"Command: {command} is not exist");
    }
}