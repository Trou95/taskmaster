using Taskmaster.Services;

namespace Taskmaster;

public interface IApp
{
     public Config config { get; set; }
     public CommandService commandService { get; set; }
     public InputService inputService { get; set; }
}

public class App : IApp
{
    public Config config { get; set; }
    public CommandService commandService { get; set; }
    public InputService inputService { get; set; }

    public App()
    {
        Console.WriteLine("App Constructor");

        //Todo: Get config path from App Constructor
        config = new Config("task.config.json");
        commandService = new CommandService();
        inputService = new InputService(this);

        InitDefaultCommands();
    }

    public void InitDefaultCommands()
    {
        commandService += "/status";
        commandService += "/start";
        commandService += "/stop";
        commandService += "/restart";
        commandService += "/reloadconfig";
    }

}
