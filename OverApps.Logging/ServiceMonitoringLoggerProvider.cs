using System;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;

namespace OverApps.Logging
{
    public sealed class ServiceMonitoringLoggerProvider : ILoggerProvider
    {
        private readonly HttpClient httpClient;
        private readonly ILoggerProvider fallbackLoggerProvider;
        private readonly ConcurrentDictionary<string, ServiceMonitoringLogger> loggers = new ConcurrentDictionary<string, ServiceMonitoringLogger>();

        private readonly ServiceMonitoringLoggerConfig config;

        public ServiceMonitoringLoggerProvider(ServiceMonitoringLoggerConfig config, HttpClient httpClient)
        {
            this.config = config;
            this.httpClient = httpClient;
        }

        public ServiceMonitoringLoggerProvider(ServiceMonitoringLoggerConfig config, HttpClient httpClient, ILoggerProvider fallbackLoggerProvider)
        {
            this.config = config;
            this.httpClient = httpClient;
            this.fallbackLoggerProvider = fallbackLoggerProvider;
        }

        public ILogger CreateLogger(string categoryName)
        {
            var fallbackLogger = fallbackLoggerProvider?.CreateLogger(categoryName);

            return loggers.GetOrAdd(categoryName, name => new ServiceMonitoringLogger(name, config, httpClient, GetFilter(name, config), fallbackLogger));
        }

        public void Dispose()
        {
            loggers.Clear();
        }

        private Func<string, LogLevel, bool> GetFilter(string name, ServiceMonitoringLoggerConfig settings)
        {
            if (settings != null)
            {
                foreach (var prefix in GetKeyPrefixes(name))
                {
                    if (settings.TryGetSwitch(prefix, out var level))
                    {
                        return (n, l) => l >= level;
                    }
                }
            }

            return (cat, level) => false;
        }

        private static IEnumerable<string> GetKeyPrefixes(string name)
        {
            while (!string.IsNullOrEmpty(name))
            {
                yield return name;
                var lastIndexOfDot = name.LastIndexOf('.');
                if (lastIndexOfDot == -1)
                {
                    yield return "Default";
                    break;
                }
                name = name.Substring(0, lastIndexOfDot);
            }
        }
    }
}
