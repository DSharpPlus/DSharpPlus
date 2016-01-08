using System;
using System.Collections.Generic;

namespace DiscordSharp
{
    public struct LoggerMessageReceivedArgs
    {
        public LogMessage message;
    }

    public struct LogMessage
    {
        public MessageLevel Level;
        public string Message;
        public DateTime TimeStamp;
    }

    public enum MessageLevel
    {
        Critical, Debug, Warning, Error
    }

    public delegate void OnLogMessageReceived(object sender, LoggerMessageReceivedArgs e);

    public class Logger
    {
        List<LogMessage> __log;

        public event OnLogMessageReceived LogMessageReceived;

        public Logger()
        {
            __log = new List<LogMessage>();
        }

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

        /// <summary>
        /// Logs a message with the given text with a default level of Debug.
        /// </summary>
        /// <param name="message"></param>
        public void Log(string message)
        {
            LogMessage m = new LogMessage();
            m.Message = message;
            m.Level = MessageLevel.Debug;
            m.TimeStamp = DateTime.Now;

            pushLog(m);
        }

        public void Log(string message, MessageLevel level)
        {
            LogMessage m = new LogMessage();
            m.Message = message;
            m.Level = level;
            m.TimeStamp = DateTime.Now;

            pushLog(m);
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
