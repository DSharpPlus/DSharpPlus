using System;

namespace DSharpPlus
{
    public class DebugLogger
    {
        public event EventHandler<DebugLogMessageEventArgs> LogMessageReceived;

        internal DebugLogger() { }

        public void LogMessage(LogLevel level, string application, string message, DateTime timestamp)
        {
            if (level >= DiscordClient.config.LogLevel)
                LogMessageReceived?.Invoke(this, new DebugLogMessageEventArgs { Level = level, Application = application, Message = message, TimeStamp = timestamp });
        }

        internal void LogHandler(object sender, DebugLogMessageEventArgs e)
        {
#if !NETSTANDARD1_1
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

            Console.Write($"[{e.Application.ToUpper()}] {e.TimeStamp} | {e.Level.ToString().ToUpper()}:");
            Console.ResetColor();
            Console.WriteLine($" {e.Message}");
#endif
        }
    }

    public class DebugLogMessageEventArgs : EventArgs
    {
        public LogLevel Level;
        public string Application;
        public string Message;
        public DateTime TimeStamp;

        public string ToString()
        {
            return $"[{Application.ToUpper()}] {TimeStamp} | {Level.ToString().ToUpper()}: {Message}";
        }
    }
}
