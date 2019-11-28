using System;

namespace OverApps.Logging.Models
{
    internal class EventLog
    {
        public object ApplicationName { get; set; }
        public string HostName { get; set; }
        public object Level { get; set; }
        public object Message { get; set; }
        public object UserName { get; set; }
        public DateTime UtcDate { get; set; }
    }
}