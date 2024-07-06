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

            if (!_commands.Add(command))
                throw new ArgumentException($"CommandService: Command ({command}) is already exist");
   
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
        }
        catch 
        {
            
        }
    }

    public bool IsCommandExist(Command command)
    {
        return _commands.Contains(command);
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
