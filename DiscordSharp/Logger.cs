using System;
using System.Collections.Generic;
using System.IO;

namespace DiscordSharp
{
	public class LoggerMessageReceivedArgs : EventArgs
    {
        public LogMessage message;
    }

    public struct LogMessage
    {
        public MessageLevel Level;
        public string Message;
        public DateTime TimeStamp;
    }
    

    [Flags]
    public enum MessageLevel : int
    {
        Critical = 0x0,
        Debug = 0x1,
        Warning = 0x2,
        Error = 0x3,
        Unecessary = 666 //for the reallllllly annoying messages
    }

    public delegate void OnLogMessageReceived(object sender, LoggerMessageReceivedArgs e);

    public class Logger
    {
        List<LogMessage> __log;

        public event OnLogMessageReceived LogMessageReceived;
        public bool EnableLogging { get; set; } = true;

        public Logger()
        {
            __log = new List<LogMessage>();
        }

        public int LogCount => __log.Count;

        void pushLog(LogMessage m)
        {
            __log.Add(m);
            if (LogMessageReceived != null)
                LogMessageReceived(this, new LoggerMessageReceivedArgs { message = m });
        }

        public void Dispose()
        {
            __log = null;
        }

        public void Save(string file)
        {
            using (var sw = new StreamWriter(file))
            {
                foreach(var log in __log)
                {
                    sw.Write($"[{log.Level.ToString()} @ {log.TimeStamp}]: {log.Message}" + Environment.NewLine);
                }
            }
        }

        public void Save(string file, MessageLevel levels)
        {
            using (var sw = new StreamWriter(file))
            {
                foreach (var log in __log)
                {
                    if(log.Level.HasFlag(levels))
                        sw.Write($"[{log.Level.ToString()} @ {log.TimeStamp}]: {log.Message}" + Environment.NewLine);
                }
            }
        }

        /// <summary>
        /// Logs a message with the given text with a default level of Debug.
        /// </summary>
        /// <param name="message"></param>
        public void Log(string message)
        {
            if (EnableLogging)
            {
                LogMessage m = new LogMessage();
                m.Message = message;
                m.Level = MessageLevel.Debug;
                m.TimeStamp = DateTime.Now;

                pushLog(m);
            }
        }

        public void Log(string message, MessageLevel level)
        {
            if (EnableLogging)
            {
                LogMessage m = new LogMessage();
                m.Message = message;
                m.Level = level;
                m.TimeStamp = DateTime.Now;

                pushLog(m);
            }
        }

        public async void LogAsync(string message, MessageLevel level)
        {
            if (EnableLogging)
            {
                LogMessage m = new LogMessage();
                m.Message = message;
                m.Level = level;
                m.TimeStamp = DateTime.Now;

                pushLog(m);
            }
        }

        /// <summary>
        /// Returns the first log at the given time.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public LogMessage GetLog(DateTime time) => __log.Find(x => x.TimeStamp == time);

        /// <summary>
        /// Returns a list of the logs with the given level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public List<LogMessage> GetLogs(MessageLevel level)
        {
            List<LogMessage> logs = new List<LogMessage>();
            __log.ForEach((obj) => 
            {
                if (obj.Level == level)
                    logs.Add(obj);
            });

            return logs;
        }

    }
}
