using System;

namespace DSharpPlus
{
    public class DebugLogger
    {
        public event EventHandler<DebugLogMessageEventArgs> LogMessageReceived;
        private LogLevel Level { get; }

        internal DebugLogger(DiscordClient client)
        {
            this.Level = client.config.LogLevel;
        }

        internal DebugLogger(LogLevel level)
        {
            this.Level = level;
        }

        public void LogMessage(LogLevel level, string application, string message, DateTime timestamp)
        {
            if (level <= this.Level)
                LogMessageReceived?.Invoke(this, new DebugLogMessageEventArgs { Level = level, Application = application, Message = message, Timestamp = timestamp });
        }

        internal void LogHandler(object sender, DebugLogMessageEventArgs e)
        {
#if !NETSTANDARD1_1
            switch (e.Level)
            {
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;

                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;

                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;

                case LogLevel.Critical:
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;
            }

            Console.Write($"[{e.Timestamp.ToString("yyyy-MM-dd HH:mm:ss zzz")}] [{e.Application}] [{e.Level}]");
            Console.ResetColor();
            Console.WriteLine($" {e.Message}");
#endif
        }
    }

    public class DebugLogMessageEventArgs : EventArgs
    {
        public LogLevel Level { get; internal set; }
        public string Application { get; internal set; }
        public string Message { get; internal set; }
        public DateTime Timestamp { get; internal set; }

        public override string ToString()
        {
            return $"[{Timestamp.ToString("yyyy-MM-dd HH:mm:ss zzz")}] [{Application}] [{Level}] {Message}";
        }
    }
}
