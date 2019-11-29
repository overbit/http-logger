using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace OverApps.Logging.Tests
{
    [TestFixture]
    public class ServiceMonitoringLoggerTests
    {
        private ServiceMonitoringLoggerConfig loggerConfig;
        private ServiceMonitoringLogger serviceMonitoringLogger;
        private HttpClient httpClient;

        private Mock<IConfiguration> configuration;
        private string expectedApplicationName;
        private string expectedLoggingEndpoint;

        [SetUp]
        public void Setup()
        {
            configuration = new Mock<IConfiguration>();
            expectedApplicationName = "MegaApplication";
            expectedLoggingEndpoint = "https://dummy.com";

            configuration.Setup(c => c["ApplicationName"]).Returns(expectedApplicationName);
            configuration.Setup(c => c["ServiceMonitoring:LoggingEndpoint"]).Returns(expectedLoggingEndpoint);
        }

        [Test]
        public void LogTest()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(rm => rm.Method == HttpMethod.Post &&
                                                        rm.Content.ReadAsStringAsync().Result.Contains("Mega warning") &&
                                                        rm.RequestUri.Equals(expectedLoggingEndpoint) &&
                                                        rm.Content.ReadAsStringAsync().Result.Contains(expectedApplicationName)),
                    ItExpr.IsAny<CancellationToken>()
                )
                // prepare the expected response of the mocked http call
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Created))
                .Verifiable();

            httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri(expectedLoggingEndpoint) };

            serviceMonitoringLogger = new ServiceMonitoringLogger("Test", new ServiceMonitoringLoggerConfig(configuration.Object), httpClient);

            serviceMonitoringLogger.Log(LogLevel.Warning, "Mega warning");

            Mock.Verify();
            Assert.Pass();
        }

        [Test]
        public void LogExceptionTest()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(rm => rm.Method == HttpMethod.Post &&
                                                        rm.RequestUri.Equals(expectedLoggingEndpoint) &&
                                                        rm.Content.ReadAsStringAsync().Result.Contains("MissingMethodException") &&
                                                        rm.Content.ReadAsStringAsync().Result.Contains("EmptyMessage")
                                                        ),
                    ItExpr.IsAny<CancellationToken>()
                )
                // prepare the expected response of the mocked http call
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Created))
                .Verifiable();

            httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri(expectedLoggingEndpoint) };

            serviceMonitoringLogger = new ServiceMonitoringLogger("Test", new ServiceMonitoringLoggerConfig(configuration.Object), httpClient);

            serviceMonitoringLogger.Log(LogLevel.Warning, new MissingMethodException(), "EmptyMessage");
            Mock.Verify();
            Assert.Pass();
        }

        [Test]
        public void LogTestFallback()
        {
            var expectedCode = System.Net.HttpStatusCode.InternalServerError;

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // prepare the expected response of the mocked http call
                .ReturnsAsync(new HttpResponseMessage(expectedCode));
            
            httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri(expectedLoggingEndpoint) };

            var fakeLogger = new FakeLogger();

            serviceMonitoringLogger = new ServiceMonitoringLogger("Test", new ServiceMonitoringLoggerConfig(configuration.Object), httpClient, null, fakeLogger);

            serviceMonitoringLogger.Log(LogLevel.Warning, "Mega warning");

            Assert.True(fakeLogger.HasLogged);
            var expectedMessage = $"services_monitoring - {expectedApplicationName} - Http request failed with status {(int)expectedCode}";
            Assert.AreEqual(expectedMessage, fakeLogger.LoggedMessage);
        }

        internal class FakeLogger : ILogger
        {
            public bool HasLogged { get; set; }
            public string LoggedMessage { get; set; }


            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                throw new NotImplementedException();
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LoggedMessage = formatter(state, exception);
                HasLogged = true;
            }
        }
    }
}