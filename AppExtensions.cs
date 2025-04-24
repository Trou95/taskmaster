
using Taskmaster;
using Taskmaster.Services;

public static class AppExtensions
{
    public static IApp AddAppServices(this IApp app)
    {
        app.commandService = new CommandService();
        app.containerService = new ContainerService();
        app.inputService = new InputService(app);

        return app;
    }

    public static IApp InitDefaultCommands(this IApp app)
    {
        app.commandService.Add("/status");
        app.commandService.Add("/start");
        app.commandService.Add("/stop");
        app.commandService.Add("/restart");
        app.commandService.Add("/reloadconfig");
        
        return app;
    }
}