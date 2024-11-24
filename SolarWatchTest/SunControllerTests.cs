using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SolarWatch.Controllers;
using SolarWatch.Services;
using SolarWatchTest.Models;

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
        public void GetSunTimes_MissingCity_ReturnsBadRequest()
        {
            var result = _controller.GetSunTimes(null);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest!.Value.ToString(), Does.Contain("The 'city' parameter is required"));
        }

        [Test]
        public void GetSunTimes_ValidCity_ReturnsSunTimes()
        {
            var city = "New York";
            var latitude = 40.7128;
            var longitude = -74.0060;
            var date = "2024-11-25";
            var sunTimes = ("06:00 AM", "06:00 PM", "07:00 AM", "07:00 PM");

            _geocodingServiceMock
                .Setup(service => service.GetCoordinates(city))
                .Returns((latitude, longitude));

            _sunriseSunsetServiceMock
                .Setup(service => service.GetSunTimes(latitude, longitude, It.IsAny<DateTime>()))
                .Returns(sunTimes);

            var result = _controller.GetSunTimes(city, date);

            Assert.IsNotNull(result, "Expected non-null result but got null.");
            Assert.IsInstanceOf<OkObjectResult>(result, "Expected result to be of type OkObjectResult.");

            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value, "Expected non-null Value in OkObjectResult but got null.");

            var response = JObject.FromObject(okResult.Value);
            Assert.AreEqual(city, response["city"].ToString());
            Assert.AreEqual("2024-11-25", response["date"].ToString());
            Assert.AreEqual("07:00 AM", response["sunrise"].ToString());
            Assert.AreEqual("07:00 PM", response["sunset"].ToString());
        }






        [Test]
        public void GetSunTimes_InvalidCity_ReturnsInternalServerError()
        {
            _geocodingServiceMock.Setup(service => service.GetCoordinates(It.IsAny<string>()))
                .Throws(new Exception("City not found"));

            var result = _controller.GetSunTimes("InvalidCity");

            Assert.IsInstanceOf<ObjectResult>(result);
            var serverError = result as ObjectResult;
            Assert.AreEqual(500, serverError!.StatusCode);
        }
    }
}

