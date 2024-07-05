using Taskmaster.Modals;

namespace Taskmaster.Services;

public class ContainerService
{
    public ContainerService() 
    {
        _containers = new List<Container>();
    }

    public ContainerService(List<Container> containers) : this()
    {
        _containers = containers;
    }

    private List<Container> _containers { get; set; }
}
