
using Taskmaster.Modals;
using Taskmaster;
using Taskmaster.Enums;
using Taskmaster.Services;

static void ApplicationStart()
{
    Config config = new Config("task.config.json");
    CommandService commandService = new CommandService();


    var containers = config.Read();
    foreach (var container in containers)
    {
        commandService.Add(container.Command, container);
    }

    Console.WriteLine(commandService.Count);
    commandService.GetCommands().ToList().ForEach(x => Console.WriteLine(x.Key));
  
}

ApplicationStart();