
using Taskmaster.Modals;
using Taskmaster;
using Taskmaster.Enums;
using Taskmaster.Services;

static void ApplicationStart()
{
    App app = new App();

    app.inputService.OnCommandTyped += OnCommandTyped;

    #region Init Default Cmds
    {
        app.commandService += "/status";
        app.commandService += "/start";
        app.commandService += "/stop";
        app.commandService += "/restart";
        app.commandService += "/reloadconfig";
    }
    #endregion

    #region Read Config
    var containers = app.config.Read();
    foreach (var container in containers)
    {
        app.commandService.Add(container.Command);
    }
    #endregion

    while(true)
    {
       app.inputService.GetInput("> ");
    }
  
}

ApplicationStart();

static void OnCommandTyped(IApp app, string command)
{ 
    Console.WriteLine($"Command: {command}");
}