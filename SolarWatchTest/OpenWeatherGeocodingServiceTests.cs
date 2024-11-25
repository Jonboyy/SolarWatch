using Moq.Protected;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SolarWatch.Models;
using SolarWatch.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;

namespace SolarWatchTest
{
    [TestFixture]
    public class OpenWeatherGeocodingServiceTests : IDisposable
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private OpenWeatherGeocodingService _service;

        [SetUp]
        public void SetUp()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://api.openweathermap.org/")
            };

            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["OpenWeatherApiKey"]).Returns("test-api-key");

            _service = new OpenWeatherGeocodingService(_httpClient, mockConfig.Object);
        }

        [Test]
        public async Task GetCoordinatesAsync_ValidCity_ReturnsGeocodingData()
        {
            var city = "New York";
            var responseContent = "[{\"lat\":40.7128,\"lon\":-74.0060}]";

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

            var result = await _service.GetCoordinatesAsync(city);

            Assert.IsNotNull(result);
            Assert.AreEqual(40.7128, result.Latitude);
            Assert.AreEqual(-74.0060, result.Longitude);
        }

        [Test]
        public void GetCoordinatesAsync_InvalidCity_ThrowsException()
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

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.GetCoordinatesAsync(city));
            Assert.AreEqual("City not found", ex.Message);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}



