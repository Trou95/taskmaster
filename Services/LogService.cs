using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskmaster.Logger;

namespace Taskmaster.Services;

public static class LogService
{
    private static List<ILogger> _loggers = new List<ILogger>();

    public static void Add(ILogger logger)
    {
        _loggers.Add(logger);
    }

    public static void Log(string message)
    {
        foreach (var logger in _loggers)
        {
            logger.Log(message);
        }
    }
}
