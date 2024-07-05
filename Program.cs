
using Taskmaster.Modals;
using Taskmaster;
using Taskmaster.Enums;

static void ApplicationStart()
{
    Config config = new Config("task.config.json");
    var containers = config.Read();

    foreach (var container in containers)
    {
        Console.WriteLine(container.Print());
    }
}

ApplicationStart();