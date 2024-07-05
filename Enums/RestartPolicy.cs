using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskmaster.Enums;

public struct RestartPolicy
{
    public static readonly RestartPolicy Always = new RestartPolicy("Always");
    public static readonly RestartPolicy Never = new RestartPolicy("Never");
    public static readonly RestartPolicy OnFailure = new RestartPolicy("OnFailure");

    private readonly string mode;

    public override string ToString()
    {
        return mode;
    }

    private RestartPolicy(string mode)
    {
        this.mode = mode;
    }
}
