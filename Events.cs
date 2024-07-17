using System.Diagnostics;
using Mono.Unix.Native;
using Taskmaster.Modals;
using Taskmaster.Services;

namespace Taskmaster;

public partial class Events
{
    public static void OnCommandTyped(IApp app, string command)
    {
        if (app.commandService.IsCommandExist(command))
        {
            if (app.commandService.IsDefaultCommand(command))
            {
                var cmd = app.commandService.ParseCommand(command);
                app.commandService.RunCommand(cmd.Item1, cmd.Item2);
            }
            else
                app.containerService.StartContainer(command);
        }
    }

    public static void OnContainerStarting(Container container)
    {
        LogService.Log($"Container: {container.Name} is starting");
    }

    public static void OnContainerSuccessfullyStart(Container container, int RunTime)
    {
        LogService.Log($"Container: {container.Name} is successfully started in {RunTime} ms");
    }

    public static void OnContainerStop(Container container)
    {
        LogService.Log($"Container: {container.Name} is stopped");
    }

    public static void OnContainerRestart(Container container, Process process)
    {
        LogService.Log($"Container: {container.Name} is restarting");
    }

    public static void OnSignalRecieved(IApp app, Signum signal)
    {
        app.containerService.SignalHandler(signal);
        LogService.Log($"Signal: {signal} is recieved");
    }
}

