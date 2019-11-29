using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System.Net.Http;

namespace OverApps.Logging.Extensions
{
    public static class ServiceMonitoringLoggerExtensions
    {
        /// <summary>
        /// Add Abcam Logging provider with EventLogLoggerProvider as fallback in case services_monitoring is down or misbehaving 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="httpClient">Used to perform the http request to services_monitoring</param>
        /// <returns></returns>
        public static ILoggingBuilder AddHttpServiceLogging(this ILoggingBuilder builder, IConfiguration configuration, HttpClient httpClient)
        {
            var config = new ServiceMonitoringLoggerConfig(configuration.GetSection("Logging"));

            builder.AddProvider(new ServiceMonitoringLoggerProvider(config, httpClient, new EventLogLoggerProvider()));

            return builder;
        }

        /// <summary>
        /// Add Abcam Logging provider
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="httpClient">Used to perform the http request to services_monitoring</param>
        /// <param name="fallbackLoggerProvider">fallback provider in case services_monitoring is down or misbehaving</param>
        /// <returns></returns>
        public static ILoggingBuilder AddHttpServiceLoggingWithLoggerFallback(this ILoggingBuilder builder, IConfiguration configuration, HttpClient httpClient, ILoggerProvider fallbackLoggerProvider)
        {
            var config = new ServiceMonitoringLoggerConfig(configuration.GetSection("Logging"));

            builder.AddProvider(new ServiceMonitoringLoggerProvider(config, httpClient, fallbackLoggerProvider));

            return builder;
        }
    }
}
