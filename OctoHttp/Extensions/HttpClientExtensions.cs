using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OctoHttp.Extensions
{
    public static class HttpClientExtensions
    {
        internal static JsonSerializer JsonSerializer = JsonSerializer.CreateDefault();

        public static Task<T> GetJsonAsync<T>(this HttpClient httpClient, Uri requestUri, CancellationToken cancellationToken = default)
        {
            return GetResponseJsonAsync<T>(httpClient, HttpMethod.Get, requestUri, null, cancellationToken);
        }

        public static Task<T> GetJsonAsync<T>(this HttpClient httpClient, string requestUri, CancellationToken cancellationToken = default)
        {
            if(requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));

            return GetJsonAsync<T>(httpClient, CreateUri(requestUri), cancellationToken);
        }

        private static async Task<T> GetResponseJsonAsync<T>(HttpClient client, HttpMethod method, Uri requestUri, object? content = default, CancellationToken cancellationToken = default)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(method, requestUri))
            {
                if (content != null)
                    request.AddJsonContent(content);

                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsJsonAsync<T>();
                }
            }
        }

        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            using Stream stream = await content.ReadAsStreamAsync();
            using StreamReader reader = new StreamReader(stream);
            using JsonReader jsonReader = new JsonTextReader(reader);

            return JsonSerializer.Deserialize<T>(jsonReader);
        }

        private static void AddJsonContent(this HttpRequestMessage request, object content)
        {
            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer.Serialize(jsonWriter, content);

                StreamContent streamContent = new StreamContent(stream);

                request.Content = streamContent;
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                request.Content.Headers.ContentEncoding.Add(Encoding.UTF8.ToString());
            }
        }

        private static Uri CreateUri(string uri)
        {
            return new Uri(uri, UriKind.RelativeOrAbsolute);
        }
    }
}