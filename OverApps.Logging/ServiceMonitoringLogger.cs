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
        private readonly ILogger fallbackLogger;

        private HttpClient httpClient { get; }

        public ServiceMonitoringLogger(string name, ServiceMonitoringLoggerConfig config, HttpClient httpClient, Func<string, LogLevel, bool> filter = null, ILogger fallbackLogger = null)
        {
            this.name = name;
            this.fallbackLogger = fallbackLogger;
            this.config = config;
            this.httpClient = httpClient;
            Filter = filter ?? ((category, logLevel) => true);
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

        public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            
            var eventLog = new LogEntry
            {
                ApplicationName = config.ApplicationName,
                HostName = Environment.MachineName,
                Level = logLevel,
                Message = $"{eventId.Id} - {name} - {formatter(state, exception)}",
                UserName = "",
                UtcDate = DateTime.UtcNow,
                ExceptionType = exception?.GetType()?.Name,
                InnerException = exception?.InnerException,
                StackTrace = exception?.StackTrace
            };

            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.abcam+json");

            var body = new StringContent(JsonSerializer.Serialize(eventLog));

            var response = await httpClient.PostAsync(config.LoggingEndpoint, body);

            if (response.StatusCode != System.Net.HttpStatusCode.Created &&
                fallbackLogger != null)
            {
                fallbackLogger.Log(LogLevel.Warning, $"services_monitoring - {eventLog.ApplicationName} - Http request failed with status {(int)response.StatusCode}");
            }
        }
    }
}
