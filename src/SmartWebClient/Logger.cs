using System;
using System.Collections.Generic;
using System.Text;

namespace SmartWebClient
{
    public class Logger
    {
        private readonly static object _lockObject = new object();

        private static bool _separateLineBreakRequired = false;
        public static void LogToConsole(LogType type, string message, bool newLine = true)
        {
            lock (_lockObject)
            {
                if (!newLine)
                {
                    _separateLineBreakRequired = true;
                    Console.Write(message);
                }
                else
                {
                    var prefix = _separateLineBreakRequired ? Environment.NewLine : "";
                    var dateString = DateTime.Now.ToString("s");
                    Console.WriteLine(prefix + dateString + " | " + type.ToString() + ": " + message);
                    _separateLineBreakRequired = false;
                }
            }
        }

        public static void LogToConsole(Exception ex)
        {
            LogToConsole(LogType.Error, ex.ToString());
        }

        public enum LogType
        {
            Information,
            Warning,
            Error,
            None
        }
    }
}
