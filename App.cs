using Mono.Unix.Native;
using Mono.Unix;
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

    public delegate void OnSignalRecieved(IApp sender, Signum signal);
    public event OnSignalRecieved? OnSignalRecievedEvent;

    public App()
    {
        config = new Config("task.config.json");
        commandService = new CommandService();
        containerService = new ContainerService();
        inputService = new InputService(this);

        InitDefaultCommands();
        SignalHandler();

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
                ExpectedRunTime = 0,
                MaxRestartAttempts = 3,
                StopSignal = 15,
                KillTimeout = 1000 * 5,
                LogOutput = false,
                EnvironmentVariables = new Dictionary<string, string>(),
                WorkingDirectory = "",
                Umask = null,
            }
        });
    }

    private void SignalHandler()
    {
        List<UnixSignal> signals = new List<UnixSignal>();
        for(int i = 1; i < 32; i++)
        {
            if(i == 9 || i == 19)
                continue;
            signals.Add(new UnixSignal((Signum)i));
        }
            

        Task.Run(() =>
        {
            while(true)
            {
                int index = UnixSignal.WaitAny(signals.ToArray());

                if (signals[index].Signum == Signum.SIGINT)
                {
                    System.Environment.Exit(0);
                }

                OnSignalRecievedEvent?.Invoke(this, signals[index].Signum);
            };

        });
    }


}
