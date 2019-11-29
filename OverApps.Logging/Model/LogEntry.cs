using System;
using Microsoft.Extensions.Logging;

namespace OverApps.Logging.Models
{
    internal class LogEntry
    {
        public string ApplicationName { get; set; }
        public string HostName { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string UserName { get; set; }
        public DateTime UtcDate { get; set; }

        public string ExceptionType { get; set; }

        public string StackTrace { get; set; }

        public Exception InnerException { get; set; }
    }
}