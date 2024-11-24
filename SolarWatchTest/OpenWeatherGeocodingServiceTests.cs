using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using SolarWatch.Services;

namespace SolarWatchTest
{
    [TestFixture]
    public class OpenWeatherGeocodingServiceTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private Mock<IConfiguration> _configurationMock;
        private OpenWeatherGeocodingService _service;

        [SetUp]
        public void SetUp()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["OpenWeatherApiKey"]).Returns("fake-api-key");

            _service = new OpenWeatherGeocodingService(_httpClient, _configurationMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
        }

        [Test]
        public void GetCoordinates_ValidCity_ReturnsCoordinates()
        {
            var city = "New York";
            var responseContent = "[{\"lat\":40.7128,\"lon\":-74.0060}]";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString() == $"http://api.openweathermap.org/geo/1.0/direct?q={city}&appid=fake-api-key"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent),
                });

            var result = _service.GetCoordinates(city);

            Assert.AreEqual(40.7128, result.Latitude);
            Assert.AreEqual(-74.0060, result.Longitude);

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public void GetCoordinates_InvalidCity_ThrowsException()
        {
            var city = "InvalidCity";
            var responseContent = "[]";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent),
                });

            var ex = Assert.Throws<Exception>(() => _service.GetCoordinates(city));
            Assert.AreEqual("City not found", ex.Message);

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
