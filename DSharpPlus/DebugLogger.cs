using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class DebugLogger
    {
        public event EventHandler<DebugLogMessageEventArgs> LogMessageReceived;

        internal DebugLogger() { }

        public void LogMessage(LogLevel level, string message, DateTime timestamp)
        {
            if (level >= DiscordClient.config.LogLevel)
                LogMessageReceived?.Invoke(this, new DebugLogMessageEventArgs() { Level = level, Message = message, TimeStamp = timestamp });
        }

        internal void LogHandler(object sender, DebugLogMessageEventArgs e)
        {
            switch(e.Level)
            {
                case LogLevel.Unnecessary:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    }
                case LogLevel.Debug:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        break;
                    }
                case LogLevel.Info:
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    }
                case LogLevel.Warning:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;
                    }
                case LogLevel.Error:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        break;
                    }
                case LogLevel.Critical:
                    {
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                    }
            }

            Console.Write($"{e.TimeStamp} | {e.Level.ToString().ToUpper()}:");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($" {e.Message}");
        }
    }

    public class DebugLogMessageEventArgs : EventArgs
    {
        public LogLevel Level;
        public string Message;
        public DateTime TimeStamp;
    }
}
