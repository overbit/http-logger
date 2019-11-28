using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OverApps.Logging
{
    public class ServiceMonitoringLoggerConfig
    {
        private readonly IConfiguration _configuration;
        public string ApplicationName => _configuration["ApplicationName"];
        public string LoggingEndpoint => _configuration["ServiceMonitoring:LoggingEndpoint"];

        public ServiceMonitoringLoggerConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public bool TryGetSwitch(string name, out LogLevel level)
        {
            var logLevels = _configuration.GetSection("LogLevel");
            if (logLevels == null)
            {
                level = LogLevel.None;
                return false;
            }

            var value = logLevels[name];
            if (string.IsNullOrEmpty(value))
            {
                level = LogLevel.None;
                return false;
            }
            else if (Enum.TryParse<LogLevel>(value, true, out level))
            {
                return true;
            }
            else
            {
                var message = $"Configuration value '{value}' for category '{name}' is not supported.";
                throw new InvalidOperationException(message);
            }
        }
    }
}