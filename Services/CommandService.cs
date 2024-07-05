using Taskmaster.Modals;

namespace Taskmaster.Services;

public class CommandService
{
    public CommandService()
    {
        _commands = new HashSet<Command>();
    }

    public CommandService(HashSet<Command> commands) : this()
    {
        _commands = commands;
    }

    public void Add(Command command)
    {
        try
        {
            if(!Command.IsValidCommand(command))
                throw new ArgumentException($"CommandService: Invalid Command ({command})");

            _commands.Add(command);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (Exception e)
        {
            
        }
    }

    public static CommandService operator +(CommandService commandService, Command command)
    {
        commandService.Add(command);
        return commandService;
    }


    public IReadOnlySet<Command> GetCommands()
    {
        return _commands;
    }

    private HashSet<Command> _commands;
    public int Count { get => _commands.Count; }

}
