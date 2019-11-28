using OverApps.Logging.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;

namespace OverApps.Logging
{
    public class ServiceMonitoringLogger : ILogger
    {
        private readonly string name;
        private Func<string, LogLevel, bool> filter;
        private readonly ServiceMonitoringLoggerConfig config;
        private HttpClient httpClient { get; }

        // TODO Add a logging processor (queue)
        public ServiceMonitoringLogger(string name, ServiceMonitoringLoggerConfig config, HttpClient httpClient, Func<string, LogLevel, bool> filter = null)
        {
            this.name = name;
            Filter = filter ?? ((category, logLevel) => true);
            this.config = config;
            this.httpClient = httpClient;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public Func<string, LogLevel, bool> Filter
        {
            get { return filter; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                filter = value;
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.None)
            {
                return false;
            }

            return Filter(name, logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            
            var eventLog = new EventLog
            {
                ApplicationName = config.ApplicationName,
                HostName = Environment.MachineName,
                Level = logLevel,
                Message = $"{eventId.Id} - {name} - {formatter(state, exception)}",
                UserName = "",
                UtcDate = DateTime.UtcNow
            };

            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.OverApps+json");

            var body = new StringContent(JsonSerializer.Serialize(eventLog));

            var response = httpClient.PostAsync(config.LoggingEndpoint, body).Result;

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                throw new Exception($"Http request failed with status {(int)response.StatusCode}");
            }
        }
    }
}
