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
        config = new Config("task.config.json");
        commandService = new CommandService();
        inputService = new InputService(this);
    }

}
