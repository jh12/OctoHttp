using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using OctoHttp.Extensions;

namespace OctoHttp.UnitTests.Extensions
{
    [TestFixture]
    public class HttpClientExtensionsTests
    {
        private HttpClient CreateMockClient(HttpStatusCode responseStatusCode, object content)
        {
            Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            string jsonContent = JsonConvert.SerializeObject(content);

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                    )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = responseStatusCode,
                    Content = new StringContent(jsonContent)
                })
                .Verifiable();

            HttpMessageHandler handler = handlerMock.Object;

            return new HttpClient(handler);
        }

        [Test]
        public async Task GetJsonAsync_ValidResponse_Pass()
        {
            // Arrange
            object response = new {key = 1, value = "string"};
            HttpClient httpClient = CreateMockClient(HttpStatusCode.OK, response);

            // Act
            KeyValuePair<int, string> result = await httpClient.GetJsonAsync<KeyValuePair<int, string>>(new Uri("http://unit.test"));

            // Assert
            result.Should().NotBeNull();
            result.Key.Should().Be(1);
            result.Value.Should().Be("string");
        }

        [Test]
        public async Task GetJsonAsyncWithString_ValidResponse_Pass()
        {
            // Arrange
            object response = new {key = 1, value = "string"};
            HttpClient httpClient = CreateMockClient(HttpStatusCode.OK, response);

            // Act
            KeyValuePair<int, string> result = await httpClient.GetJsonAsync<KeyValuePair<int, string>>("http://unit.test");

            // Assert
            result.Should().NotBeNull();
            result.Key.Should().Be(1);
            result.Value.Should().Be("string");
        }

        [Test]
        public async Task GetJsonAsync_BadRequest_ShouldThrowException()
        {
            // Arrange
            object response = new {key = 1, value = "string"};
            HttpClient httpClient = CreateMockClient(HttpStatusCode.BadRequest, response);

            // Act
            Func<Task> act = () => httpClient.GetJsonAsync<KeyValuePair<int, string>>(new Uri("http://unit.test"));

            // Assert
            (await act.Should().ThrowExactlyAsync<HttpRequestException>())
                .And.Message.Should().EndWith("400 (Bad Request).");
        }
    }
}