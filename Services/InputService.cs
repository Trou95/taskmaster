using Taskmaster.Modals;

namespace Taskmaster.Services;

public class InputService
{
    public InputService(IApp app)
    {
        _input = string.Empty;
        this.app = app;
    }

    public string? GetInput(string prefix)
    {
        _input = ReadLine.Read("> ");

        if (Command.IsValidCommand(_input!))
            OnCommandTyped?.Invoke(app, _input!);

        return _input;
    }

    public delegate void CommandHandler(IApp app, string command);
    public event CommandHandler? OnCommandTyped;

    private string? _input { get; set; }

    private IApp app;

}
