using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskmaster.Enums;

public struct ContainerStatus
{

    public static readonly ContainerStatus Running = new ContainerStatus("Running");
    public static readonly ContainerStatus Waiting = new ContainerStatus("Waiting");
    public static readonly ContainerStatus Restarting = new ContainerStatus("Restarting");

    private readonly string status;

    public static bool operator ==(ContainerStatus a, ContainerStatus b)
    {
        return a.status == b.status;
    }

    public static bool operator !=(ContainerStatus a, ContainerStatus b)
    {
        return a.status != b.status;
    }

    public override bool Equals(object? obj)
    {
        return obj is ContainerStatus status &&
               this.status == status.status;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(status);
    }

    public override string ToString()
    {
        return status;
    }

    private ContainerStatus(string status)
    {
        this.status = status;
    }
}
