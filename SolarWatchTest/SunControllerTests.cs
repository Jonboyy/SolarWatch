using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.Controllers;
using SolarWatch.Models;
using SolarWatch.Services;
using System.Threading.Tasks;

namespace SolarWatchTest
{
    [TestFixture]
    public class SunControllerTests
    {
        private Mock<IGeocodingService> _geocodingServiceMock;
        private Mock<ISunriseSunsetService> _sunriseSunsetServiceMock;
        private SunController _controller;

        [SetUp]
        public void SetUp()
        {
            _geocodingServiceMock = new Mock<IGeocodingService>();
            _sunriseSunsetServiceMock = new Mock<ISunriseSunsetService>();
            _controller = new SunController(_geocodingServiceMock.Object, _sunriseSunsetServiceMock.Object);
        }

        [Test]
        public async Task GetSunTimes_MissingCity_ReturnsBadRequest()
        {
            var request = new SunTimesRequest { City = null };

            var result = await _controller.GetSunTimes(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);

            var errorResponse = badRequest.Value as ErrorResponse;
            Assert.IsNotNull(errorResponse);

            Assert.AreEqual("Missing parameter", errorResponse.Error);
            Assert.AreEqual("The 'city' parameter is required.", errorResponse.Details);
        }

        [Test]
        public async Task GetSunTimes_ValidCity_ReturnsSunTimes()
        {
            var request = new SunTimesRequest
            {
                City = "New York",
                Date = new DateTime(2024, 11, 25),
                Timezone = "local"
            };

            var geocodingData = new GeocodingData
            {
                Latitude = 40.7128,
                Longitude = -74.0060
            };

            var sunTimes = new SunTimes
            {
                SunriseUtc = "12:00 PM",
                SunsetUtc = "10:00 PM",
                SunriseLocal = "07:00 AM",
                SunsetLocal = "05:00 PM"
            };

            _geocodingServiceMock
                .Setup(service => service.GetCoordinatesAsync(request.City))
                .ReturnsAsync(geocodingData);

            _sunriseSunsetServiceMock
                .Setup(service =>
                    service.GetSunTimesAsync(geocodingData.Latitude, geocodingData.Longitude, request.Date))
                .ReturnsAsync(sunTimes);

           
            var result = await _controller.GetSunTimes(request);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var response = okResult.Value as SunTimesResponse;
            Assert.IsNotNull(response);

            Assert.AreEqual(request.City, response.City);
            Assert.AreEqual(request.Date.ToString("yyyy-MM-dd"), response.Date);
            Assert.AreEqual(request.Timezone, response.Timezone);
            Assert.AreEqual(sunTimes.SunriseLocal, response.Sunrise);
            Assert.AreEqual(sunTimes.SunsetLocal, response.Sunset);
        }

        [Test]
        public async Task GetSunTimes_InvalidCity_ReturnsInternalServerError()
        {
            var request = new SunTimesRequest
            {
                City = "InvalidCity",
                Date = new DateTime(2024, 11, 25),
                Timezone = "local"
            };

            _geocodingServiceMock
                .Setup(service => service.GetCoordinatesAsync(request.City))
                .ThrowsAsync(new Exception("City not found"));

            var result = await _controller.GetSunTimes(request);

            Assert.IsInstanceOf<ObjectResult>(result);
            var serverError = result as ObjectResult;
            Assert.IsNotNull(serverError);
        }
    }
}
           
