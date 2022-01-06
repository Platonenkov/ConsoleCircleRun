using System;

using Microsoft.Extensions.Logging;

namespace ConsoleCircleRun
{
    public class Printer
    {
        public void Print(string message)
        {
            if (UseConsole)
                Console.WriteLine(message);
            if(_Log is not null)
                _Log.LogInformation(message);
        }

        public void PrintError(string message)
        {
            if (UseConsole)
                message.ConsoleRed();
            if (_Log is not null)
                _Log.LogError(message);
        }

        public Printer(ILogger Log = null)
        {
            _Log = Log;
        }

        public bool UseConsole = true;
        public ILogger _Log;
    }
}