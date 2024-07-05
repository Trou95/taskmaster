
namespace Taskmaster.Modals;

public readonly record struct Command(string cmd)
{
    public static implicit operator string(Command command) => command.cmd;
    public static implicit operator Command(string cmd) => new Command(cmd);

    public static bool IsValidCommand(string cmd)
    {
        if(cmd == null)
            return false;

        if(cmd.Length < 2)
            return false;

        if (cmd[0] != '/')
            return false;

        if(cmd.IndexOf(' ') != -1)
            return false;

        return true;
    }

    public override string ToString() => cmd;
}