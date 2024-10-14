using Moq;
using SolarWatch.Controllers;
using SolarWatch.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;

namespace SolarWatchTest
{
    [TestFixture]
    public class SolarWatchControllerTests
    {
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<IConfiguration> _configurationMock;
        private SolarWatchController _controller;

        [SetUp]
        public void Setup()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(config => config["GeocodingAPI:BaseUrl"])
                .Returns("https://api.openweathermap.org/geo/1.0/direct");
            _configurationMock.Setup(config => config["SunriseSunsetAPI:BaseUrl"])
                .Returns("https://api.sunrise-sunset.org/json");

            _controller = new SolarWatchController(_httpClientFactoryMock.Object, _configurationMock.Object);
        }

        [Test]
        public async Task GetSunriseSunset_ValidCityAndDate_ReturnsOkResult()
        {
            // Arrange
            var date = "2024-10-14";

            // Setup mocked Geocoding API response
            var geocodingResponse = "[{\"name\": \"Budapest\", \"lat\": 47.4979, \"lon\": 19.0402, \"country\": \"HU\"}]";
            var geocodingHttpClient = new HttpClient(new MockHttpMessageHandler(geocodingResponse))
            {
                BaseAddress = new Uri("https://api.openweathermap.org")
            };
            _httpClientFactoryMock.Setup(factory => factory.CreateClient("GeocodingClient"))
                .Returns(geocodingHttpClient);

            // Setup mocked Sunrise-Sunset API response
            var sunsetResponse = "{\"results\": { \"sunrise\": \"2024-10-14T04:59:48+00:00\", \"sunset\": \"2024-10-14T15:59:58+00:00\"}, \"status\": \"OK\"}";
            var sunsetHttpClient = new HttpClient(new MockHttpMessageHandler(sunsetResponse))
            {
                BaseAddress = new Uri("https://api.sunrise-sunset.org")
            };
            _httpClientFactoryMock.Setup(factory => factory.CreateClient("SunsetClient"))
                .Returns(sunsetHttpClient);

            // Act
            var result = await _controller.GetSunriseSunset("Budapest", date) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null, "Expected OkObjectResult, but got null.");

            // Cast result.Value to SunriseSunsetResult
            var response = result.Value as SunriseSunsetResult;

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null, "Expected response data, but got null.");
                Assert.That(response.Sunrise, Is.EqualTo("2024-10-14T04:59:48+00:00"));
                Assert.That(response.Sunset, Is.EqualTo("2024-10-14T15:59:58+00:00"));
            });
        }
    }
}





