using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace OverApps.Logging.Tests
{
    [TestFixture]
    public class ServiceMonitoringLoggerConfigTests
    {
        Mock<IConfiguration> configuration;

        [OneTimeSetUp]
        public void SetUpAttribute()
        {
            configuration = new Mock<IConfiguration>();
        }

        [Test]
        public void ServiceMonitoringLoggerConfigTest()
        {
            // arrange
            configuration.Setup(c => c["ApplicationName"]).Returns("ApplicationName");
            configuration.Setup(c => c["ServiceMonitoring:LoggingEndpoint"]).Returns("https://LoggingEndpoint");
            
            var logConfigSection = new Mock<IConfigurationSection>();
            logConfigSection.SetupGet(v => v.Value).Returns("{ \"Default\": \"Warning\", \"Microsoft\": \"Warning\", \"Microsoft.Hosting.Lifetime\": \"Information\" }");
            configuration.Setup(c => c.GetSection("LogLevel")).Returns(logConfigSection.Object);

            // act
            var config = new ServiceMonitoringLoggerConfig(configuration.Object);

            // assert
            Assert.AreEqual("ApplicationName", config.ApplicationName);
            Assert.AreEqual("https://LoggingEndpoint", config.LoggingEndpoint);
        }

        [Test]
        public void Settings_LogLevelIgnoreCase()
        {
            var section = new Mock<IConfigurationSection>();
            section.SetupGet(x => x["MyTest"])
                .Returns("INFOrmAtiOn");

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(x => x.GetSection("LogLevel"))
                .Returns(section.Object);

            var settings = new ServiceMonitoringLoggerConfig(configuration.Object);

            // Act
            LogLevel logLevel = LogLevel.None;
            settings.TryGetSwitch("MyTest", out logLevel);

            // Assert
            Assert.AreEqual(LogLevel.Information, logLevel);
        }

        [Test]
        public void Settings_GetTheRightLogLevel()
        {
            var expected = LogLevel.Trace;

            var section = new Mock<IConfigurationSection>();
            section.SetupGet(x => x["MyTest"])
                .Returns("INFOrmAtiOn");
            section.SetupGet(x => x["Default"])
                .Returns(expected.ToString());

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(x => x.GetSection("LogLevel"))
                .Returns(section.Object);

            var settings = new ServiceMonitoringLoggerConfig(configuration.Object);

            // Act
            LogLevel logLevel = LogLevel.None;
            settings.TryGetSwitch("Default", out logLevel);

            // Assert
            Assert.AreEqual(expected, logLevel);
        }
    }
}