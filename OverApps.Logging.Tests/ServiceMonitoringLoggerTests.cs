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

        [SetUp]
        public void Setup()
        {
            configuration = new Mock<IConfiguration>();
        }
        
        [Test]
        public void LogTest()
        {
            var expectedApplicationName = "MegaApplication";
            var expectedLoggingEndpoint = "https://dummy.com";

            configuration.Setup(c => c["ApplicationName"]).Returns(expectedApplicationName);
            configuration.Setup(c => c["ServiceMonitoring:LoggingEndpoint"]).Returns(expectedLoggingEndpoint);

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
    }
}