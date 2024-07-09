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
            app.containerService.StartContainer(command);
        }
        else
        {
            //Console.WriteLine($"Command: {command} is not exist");
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

    public static void OnProcessExit(Process process)
    {
        LogService.Log($"Process: {process.Id} is exited");
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

