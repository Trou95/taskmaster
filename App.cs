using Taskmaster.Enums;
using Taskmaster.Modals;
using Taskmaster.Services;

namespace Taskmaster;

public interface IApp
{
     public Config config { get; }
     public CommandService commandService { get; }
     public InputService inputService { get; }

     public ContainerService containerService { get; }
}

public class App : IApp
{
    public  Config config { get; }
    public CommandService commandService { get; }
    public InputService inputService { get; }
    public ContainerService containerService { get; }

    public App()
    {
        config = new Config("task.config.json");
        commandService = new CommandService();
        containerService = new ContainerService();
        inputService = new InputService(this);

        InitDefaultCommands();

        if(Environment.GetCommandLineArgs().Length > 1)
        {
            var arg = Environment.GetCommandLineArgs()[1];
            if(arg == "--test")
            {

               if(containerService.Count == 0)
                   InitDefaultContainer(this);
            }
        }
    }

    public void InitDefaultCommands()
    {
        commandService.Add("/status");
        commandService.Add("/start");
        commandService.Add("/stop");
        commandService.Add("/restart");
        commandService.Add("/reloadconfig");
    }

    private static void InitDefaultContainer(IApp app)
    {
        app.config.Write(new List<Container>
    {
        new Container
        {
            Name = "Test",
            Command = "/test",
            BinaryPath = "/bin/ls",
            NumberOfProcesses = 1,
            StartAtLaunch = false,
            RestartPolicy = RestartPolicy.Never,
            ExpectedExitCodes = new List<int> { 0 },
            ExpectedExitCode = 0,
            StartTimeout = 1000 * 5,
            MaxRestartAttempts = 3,
            StopSignal = 15,
            KillTimeout = 1000 * 5,
            LogOutput = false,
            EnvironmentVariables = new Dictionary<string, string>(),
            WorkingDirectory = "",
            Umask = 0
        }
    });
    }


}
