using Taskmaster.Modals;

namespace Taskmaster.Services;

public class CommandService
{
    public CommandService()
    {
        _commands = new Dictionary<Command, Container>();
    }

    public CommandService(Dictionary<Command, Container> commands) : this()
    {
        _commands = commands;
    }

    public void Add(Command command, Container container)
    {
        try
        {
            if(!Command.IsValidCommand(command))
                throw new ArgumentException($"CommandService: Invalid Command ({command})");

            _commands.Add(command, container);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (Exception e)
        {
            
        }
    }

    public void Add(Tuple<Command, Container> command)
    {
        Add(command.Item1, command.Item2);
    }

    public static CommandService operator +(CommandService commandService, Tuple<Command, Container> command)
    {
        commandService.Add(command.Item1, command.Item2);
        return commandService;
    }


    public IReadOnlyDictionary<Command, Container> GetCommands() 
    {
        return _commands;
    }

    private Dictionary<Command, Container> _commands;
    public int Count { get => _commands.Count; }

}
