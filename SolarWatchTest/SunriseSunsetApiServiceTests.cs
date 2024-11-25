using Moq.Protected;
using NUnit.Framework;
using SolarWatch.Models;
using SolarWatch.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace SolarWatchTest
{
    [TestFixture]
    public class SunriseSunsetApiServiceTests : IDisposable
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private SunriseSunsetApiService _service;

        [SetUp]
        public void SetUp()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.sunrise-sunset.org/")
            };

            _service = new SunriseSunsetApiService(_httpClient);
        }

        [Test]
        public async Task GetSunTimesAsync_ValidCoordinates_ReturnsSunTimes()
        {
            var latitude = 40.7128;
            var longitude = -74.0060;
            var date = new DateTime(2024, 11, 25);

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            var responseContent = @"
            {
                'results': {
                    'sunrise': '2024-11-25T12:00:00+00:00',
                    'sunset': '2024-11-25T22:00:00+00:00'
                },
                'status': 'OK'
            }";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent),
                });

            var result = await _service.GetSunTimesAsync(latitude, longitude, date, timeZoneInfo);

            Assert.IsNotNull(result);
            Assert.AreEqual("12:00 PM", result.SunriseUtc);   
            Assert.AreEqual("10:00 PM", result.SunsetUtc);       
            Assert.AreEqual("07:00 AM", result.SunriseLocal);   
            Assert.AreEqual("05:00 PM", result.SunsetLocal);     
        }

        [Test]
        public void GetSunTimesAsync_InvalidResponse_ThrowsException()
        {
            var latitude = 0.0;
            var longitude = 0.0;
            var date = new DateTime(2024, 11, 25);
            var timeZoneInfo = TimeZoneInfo.Utc;

            var responseContent = @"{ 'status': 'INVALID_REQUEST' }";

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

            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _service.GetSunTimesAsync(latitude, longitude, date, timeZoneInfo));

            Assert.AreEqual("Failed to fetch sunrise/sunset data", ex.Message);
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





