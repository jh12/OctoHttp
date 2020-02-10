using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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
        private HttpClient CreateMockClient(HttpStatusCode responseStatusCode, object content, HttpMethod requestingMethod)
        {
            string jsonContent = JsonConvert.SerializeObject(content);

            return CreateMockClient(message =>
            {
                if(message.Method != requestingMethod)
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);

                return new HttpResponseMessage
                {
                    StatusCode = responseStatusCode,
                    Content = new StringContent(jsonContent)
                };
            });
        }

        private HttpClient CreateMockClient(Func<HttpRequestMessage, HttpResponseMessage> responseFunc)
        {
            Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync((HttpRequestMessage request, CancellationToken cancellationToken) => responseFunc(request))
                .Verifiable();

            HttpMessageHandler handler = handlerMock.Object;

            return new HttpClient(handler);
        }

        #region Get

        [Test]
        public async Task GetJsonAsync_ValidResponse_Pass()
        {
            // Arrange
            object response = new {key = 1, value = "string"};
            HttpClient httpClient = CreateMockClient(HttpStatusCode.OK, response, HttpMethod.Get);

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
            HttpClient httpClient = CreateMockClient(HttpStatusCode.OK, response, HttpMethod.Get);

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
            HttpClient httpClient = CreateMockClient(HttpStatusCode.BadRequest, response, HttpMethod.Get);

            // Act
            Func<Task> act = () => httpClient.GetJsonAsync<KeyValuePair<int, string>>(new Uri("http://unit.test"));

            // Assert
            (await act.Should().ThrowExactlyAsync<HttpRequestException>())
                .And.Message.Should().EndWith("400 (Bad Request).");
        }

        #endregion

        #region Post

        [Test]
        public async Task PostJsonAsync_NoBodyValidResponse_Pass()
        {
            // Arrange
            object response = new {key = 1, value = "string"};
            HttpClient httpClient = CreateMockClient(HttpStatusCode.OK, response, HttpMethod.Post);

            // Act
            KeyValuePair<int, string> result = await httpClient.PostJsonAsync<KeyValuePair<int, string>>(new Uri("http://unit.test"), null);

            // Assert
            result.Should().NotBeNull();
            result.Key.Should().Be(1);
            result.Value.Should().Be("string");
        }

        [Test]
        public async Task PostJsonAsync_ValidRequestBody_Pass()
        {
            // Arrange
            object body = new {key = 1, value = "string"};

            Func<HttpRequestMessage, HttpResponseMessage> responseFunc = message =>
            {
                if(message.Method != HttpMethod.Post)
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);

                if(message.Content.Headers.ContentType.MediaType != "application/json")
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);

                if(message.Content.Headers.ContentType.CharSet != Encoding.UTF8.WebName)
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);

                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                string responseContent = message.Content.ReadAsStringAsync().Result;
                response.Content = new StringContent(responseContent, Encoding.UTF8, "application/json");

                return response;
            };

            HttpClient httpClient = CreateMockClient(responseFunc);

            // Act
            KeyValuePair<int, string> result = await httpClient.PostJsonAsync<KeyValuePair<int, string>>(new Uri("http://unit.test"), body);

            // Assert
            result.Should().NotBeNull();
            result.Key.Should().Be(1);
            result.Value.Should().Be("string");
        }

        #endregion

        #region Put

        [Test]
        public async Task PutJsonAsync_NoBodyValidResponse_Pass()
        {
            // Arrange
            object response = new {key = 1, value = "string"};
            HttpClient httpClient = CreateMockClient(HttpStatusCode.OK, response, HttpMethod.Put);

            // Act
            KeyValuePair<int, string> result = await httpClient.PutJsonAsync<KeyValuePair<int, string>>(new Uri("http://unit.test"), null);

            // Assert
            result.Should().NotBeNull();
            result.Key.Should().Be(1);
            result.Value.Should().Be("string");
        }

        #endregion

        #region Put

        [Test]
        public async Task DeleteJsonAsync_NoBodyValidResponse_Pass()
        {
            // Arrange
            object response = new {key = 1, value = "string"};
            HttpClient httpClient = CreateMockClient(HttpStatusCode.OK, response, HttpMethod.Delete);

            // Act
            KeyValuePair<int, string> result = await httpClient.DeleteJsonAsync<KeyValuePair<int, string>>(new Uri("http://unit.test"), null);

            // Assert
            result.Should().NotBeNull();
            result.Key.Should().Be(1);
            result.Value.Should().Be("string");
        }

        #endregion
    }
}