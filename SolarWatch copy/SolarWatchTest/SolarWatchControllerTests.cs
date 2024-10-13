using Moq;
using SolarWatch.Controllers;
using SolarWatch.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


namespace SolarWatchTest
{
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
            _configurationMock.Setup(config => config["SunriseSunsetAPI:BaseUrl"])
                .Returns("https://api.sunrise-sunset.org/json");

            _controller = new SolarWatchController(_httpClientFactoryMock.Object, _configurationMock.Object);
        }

        [Test]
        public async Task GetSunriseSunset_ValidCityAndDate_ReturnsOkResult()
        {
            // Arrange
            var date = new DateTime(2024, 10, 14);

            // Mock the Geocoding API response for the GeocodingClient
            var geocodingResponse = "[{\"name\": \"Budapest\", \"lat\": 47.4979, \"lon\": 19.0402, \"country\": \"HU\"}]";
            var geocodingHttpClient = new HttpClient(new MockHttpMessageHandler(geocodingResponse))
            {
                BaseAddress = new Uri("https://api.openweathermap.org")
            };
            _httpClientFactoryMock.Setup(factory => factory.CreateClient("GeocodingClient"))
                .Returns(geocodingHttpClient);

            // Mock the Sunrise-Sunset API response for the SunsetClient
            var sunsetResponse = "{\"results\": { \"sunrise\": \"2024-10-14T04:59:48+00:00\", \"sunset\": \"2024-10-14T15:59:58+00:00\"}, \"status\": \"OK\"}";
            var sunsetHttpClient = new HttpClient(new MockHttpMessageHandler(sunsetResponse))
            {
                BaseAddress = new Uri("https://api.sunrise-sunset.org")
            };
            _httpClientFactoryMock.Setup(factory => factory.CreateClient("SunsetClient"))
                .Returns(sunsetHttpClient);

            // Act
            var result = await _controller.GetSunriseSunset("Budapest", date) as OkObjectResult;
            var response = result?.Value as SunriseSunsetResponse;

            // Assert
            
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(200));
                Assert.That(response, Is.Not.Null);
            });
        }


    }
}