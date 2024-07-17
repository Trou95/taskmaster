﻿using Mono.Unix.Native;
using Mono.Unix;
using Taskmaster.Enums;
using Taskmaster.Modals;
using Taskmaster.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

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
        commandService.OnDefaultCommand += OnDefaultCommand;

        SignalHandler();
        SocketHandler();

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
            if(i == 9 || i == 17 || i == 19)
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

    private void SocketHandler()
    {
        IPAddress ipAddress = IPAddress.Any;
        int port = 8080;

        TcpListener listener = new TcpListener(ipAddress, port);
        listener.Start();

        Task.Run(() =>
        {
            while(true)
            {
                TcpClient client = listener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                Task.Run(async () =>
                {

                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead;


                        while (true)
                        {

                            bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                            if (bytesRead == 0)
                                break;

                            string request = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            if (commandService.IsCommandExist(request))
                            {
                                containerService.StartContainer(request);
                            }
                        }
                    }
                       
                });
               
            }
        });

    }

    private void OnDefaultCommand(Command command)
    {
        if(command == "/status")
        {
            containerService.PrintContainers();
        }
    }
}
