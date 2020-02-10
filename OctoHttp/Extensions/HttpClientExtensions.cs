using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OctoHttp.Extensions
{
    public static class HttpClientExtensions
    {
        internal static JsonSerializer JsonSerializer = JsonSerializer.CreateDefault();

        #region Get

        public static Task<T> GetJsonAsync<T>(this HttpClient httpClient, Uri requestUri, CancellationToken cancellationToken = default)
        {
            return GetResponseJsonAsync<T>(httpClient, HttpMethod.Get, requestUri, null, cancellationToken);
        }

        public static Task<T> GetJsonAsync<T>(this HttpClient httpClient, string requestUri, CancellationToken cancellationToken = default)
        {
            return GetResponseJsonAsync<T>(httpClient, HttpMethod.Get, requestUri, null, cancellationToken);
        }

        #endregion

        #region Post

        public static Task<T> PostJsonAsync<T>(this HttpClient httpClient, Uri requestUri, object? content, CancellationToken cancellationToken = default)
        {
            return GetResponseJsonAsync<T>(httpClient, HttpMethod.Post, requestUri, content, cancellationToken);
        }

        public static Task<T> PostJsonAsync<T>(this HttpClient httpClient, string requestUri, object? content, CancellationToken cancellationToken = default)
        {
            return GetResponseJsonAsync<T>(httpClient, HttpMethod.Post, requestUri, content, cancellationToken);
        }

        #endregion

        #region Put

        public static Task<T> PutJsonAsync<T>(this HttpClient httpClient, Uri requestUri, object? content, CancellationToken cancellationToken = default)
        {
            return GetResponseJsonAsync<T>(httpClient, HttpMethod.Put, requestUri, content, cancellationToken);
        }

        public static Task<T> PutJsonAsync<T>(this HttpClient httpClient, string requestUri, object? content, CancellationToken cancellationToken = default)
        {
            return GetResponseJsonAsync<T>(httpClient, HttpMethod.Put, requestUri, content, cancellationToken);
        }

        #endregion

        #region Delete

        public static Task<T> DeleteJsonAsync<T>(this HttpClient httpClient, Uri requestUri, object? content, CancellationToken cancellationToken = default)
        {
            return GetResponseJsonAsync<T>(httpClient, HttpMethod.Delete, requestUri, content, cancellationToken);
        }

        public static Task<T> DeleteJsonAsync<T>(this HttpClient httpClient, string requestUri, object? content, CancellationToken cancellationToken = default)
        {
            return GetResponseJsonAsync<T>(httpClient, HttpMethod.Delete, requestUri, content, cancellationToken);
        }

        #endregion

        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            using Stream stream = await content.ReadAsStreamAsync();
            using StreamReader reader = new StreamReader(stream);
            using JsonReader jsonReader = new JsonTextReader(reader);

            return JsonSerializer.Deserialize<T>(jsonReader);
        }

        #region Private Methods

        private static Task<T> GetResponseJsonAsync<T>(HttpClient client, HttpMethod method, string requestUri, object? content = default, CancellationToken cancellationToken = default)
        {
            if(requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));

            return GetResponseJsonAsync<T>(client, method, CreateUri(requestUri), content, cancellationToken);
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

        private static void AddJsonContent(this HttpRequestMessage request, object content)
        {
            string json = JsonConvert.SerializeObject(content);

            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static Uri CreateUri(string uri)
        {
            return new Uri(uri, UriKind.RelativeOrAbsolute);
        }

        #endregion
    }
}