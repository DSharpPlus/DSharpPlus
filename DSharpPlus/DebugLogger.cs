using System;
using DSharpPlus.EventArgs;

namespace DSharpPlus
{
    public class DebugLogger
    {
        public event EventHandler<DebugLogMessageEventArgs> LogMessageReceived;
        private LogLevel Level { get; }
        private string DateTimeFormat { get; }

        internal DebugLogger(BaseDiscordClient client)
        {
            this.Level = client.Configuration.LogLevel;
            this.DateTimeFormat = client.Configuration.DateTimeFormat;
        }

        internal DebugLogger(LogLevel level, string timeformatting)
        {
            this.Level = level;
            this.DateTimeFormat = timeformatting;
        }

        public void LogMessage(LogLevel level, string application, string message, DateTime timestamp)
        {
            if (level <= this.Level)
                LogMessageReceived?.Invoke(this, new DebugLogMessageEventArgs { Level = level, Application = application, Message = message, Timestamp = timestamp, TimeFormatting = this.DateTimeFormat });
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

            Console.Write($"[{e.Timestamp.ToString(this.DateTimeFormat)}] [{e.Application}] [{e.Level}]");
            Console.ResetColor();
            Console.WriteLine($" {e.Message}");
#endif
        }
    }

    namespace EventArgs
    {
        /// <summary>
        /// Represents arguments for <see cref="DebugLogger.LogMessageReceived"/> event.
        /// </summary>
        public class DebugLogMessageEventArgs : System.EventArgs
        {
            /// <summary>
            /// Gets the level of the message.
            /// </summary>
            public LogLevel Level { get; internal set; }

            /// <summary>
            /// Gets the name of the application which generated the message.
            /// </summary>
            public string Application { get; internal set; }

            /// <summary>
            /// Gets the sent message.
            /// </summary>
            public string Message { get; internal set; }

            /// <summary>
            /// Gets the timestamp of the message.
            /// </summary>
            public DateTime Timestamp { get; internal set; }

            internal string TimeFormatting;

            public override string ToString()
            {
                return $"[{Timestamp.ToString(this.TimeFormatting)}] [{Application}] [{Level}] {Message}";
            }
        }
    }
}
