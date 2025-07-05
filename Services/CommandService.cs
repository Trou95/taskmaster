using Taskmaster.Modals;

namespace Taskmaster.Services;

public class CommandService
{
    public CommandService()
    {
        _commands = new HashSet<Command>();
        ReadLine.AutoCompletionHandler = new AutoCompletionHandler();
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

    public void UpdateCommands(List<Container> containers)
    {
        var existingCommands = _commands.Where(c => !c.isDefault).ToList();
        
        foreach (var cmd in existingCommands)
        {
            _commands.Remove(cmd);
        }

        foreach (var container in containers)
        {
            Add(new Command(container.Command, false));
        }
    }


    public void RunCommand(string command, List<string> args)
    {
        OnDefaultCommand?.Invoke(command, args);
    }

    public bool IsCommandExist(Command command)
    {
        var cmd = CommandSplit(command.cmd);
        return _commands.Where(c => CommandSplit(c.cmd) == cmd).Count() > 0;
    }

    public bool IsDefaultCommand(Command command)
    {
        return IsCommandExist(command) && command.isDefault;
    }

    public string CommandSplit(string command)
    {
        if(command.Contains(" "))
            return command.Split(" ")[0];
        return command;
    }

    public Tuple<string, List<string>> ParseCommand(Command command)
    {
       if(command.cmd.Contains(" "))
       {
           var cmd = command.cmd.Split(" ");
           return new Tuple<string, List<string>>(cmd[0], cmd.Skip(1).ToList());
       }
       return new Tuple<string, List<string>>(command.cmd, new List<string>());
    }

    public static CommandService operator +(CommandService commandService, Command command)
    {
        commandService.Add(command);
        return commandService;
    }

    public event Action<Command, List<string>>? OnDefaultCommand;

    public IReadOnlySet<Command> GetCommands()
    {
        return _commands;
    }

    private HashSet<Command> _commands;
    public int Count { get => _commands.Count; }

    class AutoCompletionHandler : IAutoCompleteHandler
    {
        public char[] Separators { get; set; } = new char[] {  };
    
        public string[] GetSuggestions(string text, int index)
        {
            if (text.StartsWith("/"))
                return new string[] { "/status", "/start", "/stop", "/restart", "/reloadconfig", "/quit", "/help" };
            else
                return ["/"];
        }
    }

}
