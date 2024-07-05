using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskmaster.Enums;

public struct ProcessStatus
{

    public static readonly ProcessStatus Running = new ProcessStatus("Running");
    public static readonly ProcessStatus Waiting = new ProcessStatus("Waiting");

    private readonly string status;

    public override string ToString()
    {
        return status;
    }

    private ProcessStatus(string status)
    {
        this.status = status;
    }
}
