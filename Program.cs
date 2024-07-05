
using Taskmaster.Modals;
using Taskmaster;
using Taskmaster.Enums;
using Taskmaster.Services;

static void ApplicationStart()
{
    Config config = new Config("task.config.json");
    CommandService commandService = new CommandService();

    #region Init Default Cmds
    {
        commandService += "/status";
        commandService += "/start";
        commandService += "/stop";
        commandService += "/restart";
        commandService += "/reloadconfig";
    }
    #endregion

    #region Read Config
    var containers = config.Read();
    foreach (var container in containers)
    {
        commandService.Add(container.Command);
    }
    #endregion

  
}

ApplicationStart();