using System.Net.Http;
using System.Xml.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace OverApps.Logging.Tests
{
    [TestFixture]
    public class ServiceMonitoringLoggerProviderTests
    {
        [Test]
        [TestCase("Default", LogLevel.Error)]
        [TestCase("OverApps.Logging", LogLevel.Warning)]
        [TestCase("OverApps.Logging.SomethingElse", LogLevel.Information)]
        public void MatchingProviderFilters(string categoryName, LogLevel actuaLogLevel)
        {
            // Arrange
            var json =
                @"{
                  ""LogLevel"": {
                    ""Default"": ""Error"",
                    ""OverApps.Logging"": ""Warning"",
                    ""OverApps.Logging.SomethingElse"": ""Trace"",
                  },
                  ""ApplicationName"" : ""TestApp""
                }";

            var config = TestConfiguration.Create(() => json);

            var loggerConfig = new ServiceMonitoringLoggerConfig(config);

            // Act
            using var provider = new ServiceMonitoringLoggerProvider(loggerConfig, null);
            var logger = provider.CreateLogger(categoryName);

            // Assert
            Assert.True(logger.IsEnabled(actuaLogLevel));
        }

        [Test]
        [TestCase("OverApps.NewLogging", LogLevel.Warning)]
        [TestCase("Another.Name", LogLevel.Information)]
        public void NotMatchingProviderFilters(string categoryName, LogLevel actuaLogLevel)
        {
            // Arrange
            var json =
                @"{
                  ""LogLevel"": {
                    ""OverApps.Logging"": ""Warning"",
                    ""OverApps.Logging.SomethingElse"": ""Trace"",
                  },
                  ""ApplicationName"" : ""TestApp""
                }";

            var config = TestConfiguration.Create(() => json);
            var loggerConfig = new ServiceMonitoringLoggerConfig(config);

            // Act
            using var provider = new ServiceMonitoringLoggerProvider(loggerConfig, null);
            var logger = provider.CreateLogger(categoryName);

            // Assert
            Assert.False(logger.IsEnabled(actuaLogLevel));
        }

        [Test]
        [TestCase("OverApps.Logging.Another", LogLevel.Warning)]
        [TestCase("OverApps.Logging.SomethingElse.SomethingElse", LogLevel.Information)]
        public void FallbackMatchingProviderFilters(string categoryName, LogLevel actuaLogLevel)
        {
            // Arrange
            var json =
                @"{
                  ""LogLevel"": {
                    ""OverApps.Logging"": ""Warning"",
                    ""OverApps.Logging.SomethingElse"": ""Trace"",
                  },
                  ""ApplicationName"" : ""TestApp""
                }";

            var config = TestConfiguration.Create(() => json);
            var loggerConfig = new ServiceMonitoringLoggerConfig(config);

            // Act
            using var provider = new ServiceMonitoringLoggerProvider(loggerConfig, null);
            var logger = provider.CreateLogger(categoryName);

            // Assert
            Assert.True(logger.IsEnabled(actuaLogLevel));
        }

        [Test]
        [TestCase("NewLogging.Something", LogLevel.Warning)]
        public void FallbackToDefaultForNotMatchingProviderFilters(string categoryName, LogLevel actuaLogLevel)
        {
            // Arrange
            var json =
                @"{
                  ""LogLevel"": {
                    ""Default"": ""Warning"",
                    ""OverApps.Logging.SomethingElse"": ""Trace"",
                  },
                  ""ApplicationName"" : ""TestApp""
                }";

            var config = TestConfiguration.Create(() => json);
            var loggerConfig = new ServiceMonitoringLoggerConfig(config);

            // Act
            using var provider = new ServiceMonitoringLoggerProvider(loggerConfig, null);
            var logger = provider.CreateLogger(categoryName);

            // Assert
            Assert.True(logger.IsEnabled(actuaLogLevel));
        }
    }
}