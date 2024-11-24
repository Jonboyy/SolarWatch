using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SolarWatch.Services;

namespace SolarWatchTest
{
    [TestFixture]
    public class SunriseSunsetApiServiceTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private SunriseSunsetApiService _service;

        [SetUp]
        public void SetUp()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _service = new SunriseSunsetApiService(_httpClient);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
        }

        [Test]
        public void GetSunTimes_ValidCoordinates_ReturnsSunTimes()
        {
            // Arrange
            var latitude = 40.7128;
            var longitude = -74.0060;
            var date = new DateTime(2024, 11, 25);

            var responseContent = @"{
        'results': {
            'sunrise': '2024-11-25T12:00:00+00:00',
            'sunset': '2024-11-25T22:00:00+00:00'
        },
        'status': 'OK'
    }";

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

            var result = _service.GetSunTimes(latitude, longitude, date);

            var sunriseUtc = DateTime.SpecifyKind(DateTime.Parse("2024-11-25T12:00:00+00:00"), DateTimeKind.Utc);
            var sunsetUtc = DateTime.SpecifyKind(DateTime.Parse("2024-11-25T22:00:00+00:00"), DateTimeKind.Utc);

            var sunriseLocal = sunriseUtc.ToLocalTime();
            var sunsetLocal = sunsetUtc.ToLocalTime();

            Assert.AreEqual(sunriseUtc.ToString("hh:mm tt"), result.SunriseUtc);
            Assert.AreEqual(sunsetUtc.ToString("hh:mm tt"), result.SunsetUtc);
            Assert.AreEqual(sunriseLocal.ToString("hh:mm tt"), result.SunriseLocal);
            Assert.AreEqual(sunsetLocal.ToString("hh:mm tt"), result.SunsetLocal);
        }



        [Test]
        public void GetSunTimes_InvalidResponse_ThrowsException()
        {
            var latitude = 40.7128;
            var longitude = -74.0060;
            var date = new DateTime(2024, 11, 25);

            var responseContent = @"{ 'status': 'INVALID_REQUEST' }";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(responseContent),
                });

            var ex = Assert.Throws<AggregateException>(() => _service.GetSunTimes(latitude, longitude, date));
            Assert.IsInstanceOf<HttpRequestException>(ex.InnerException);
            Assert.AreEqual("Response status code does not indicate success: 400 (Bad Request).", ex.InnerException.Message);
        }
    }
}

