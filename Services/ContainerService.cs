using System.Diagnostics;
using AutoMapper;
using Taskmaster.Modals;

namespace Taskmaster.Services;

public class ContainerService
{
    public ContainerService() 
    {
        _containers = new List<ExtendedContainer>();
        _mapper = new MapperConfiguration(cfg => { 
            cfg.CreateMap<Container, ExtendedContainer>(); 
        }).CreateMapper();
    }

    public ContainerService(List<Container> containers) : this()
    {
        _containers = _mapper.Map<List<ExtendedContainer>>(containers);
    }

    public void StartContainer(Command command)
    {
        try
        {
            var container = FindByCommand(command);

        }
        catch (ContainerServiceException e)
        {
            Console.WriteLine(e.Message);
        }

    }

    public Container? FindByCommand(Command command)
    {
        return _containers.Find(c => c.Command == command);
    }

    private List<ExtendedContainer> _containers { get; set; }

    private readonly IMapper _mapper;

    private class ExtendedContainer : Container
    {
        Process? Process { get; set; }
    }

    private class ContainerServiceException : Exception 
    { 
        public ContainerServiceException(string message) : base(message) { }
    }

    private class ContainerNotFoundException : ContainerServiceException 
    { 
        public ContainerNotFoundException(string message) : base(message) { }
    }
}
