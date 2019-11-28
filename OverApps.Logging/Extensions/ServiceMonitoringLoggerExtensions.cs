using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace OverApps.Logging.Extensions
{
    public static class ServiceMonitoringLoggerExtensions
    {
        public static ILoggingBuilder AddHttpServiceLogging(this ILoggingBuilder builder, IConfiguration configuration, HttpClient httpClient)
        {
            var config = new ServiceMonitoringLoggerConfig(configuration.GetSection("Logging"));

            builder.AddProvider(new ServiceMonitoringLoggerProvider(config, httpClient));

            return builder;
        }
    }
}
