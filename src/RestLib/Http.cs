using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;

namespace RestLib
{
    public class Http : IHttp
    {
        public static Func<HttpClient> ClientFactory =
            () =>
                PerRequestHandler != null
                    ? new HttpClient(PerRequestHandler, false)
                    : new HttpClient(DefaultHandler, false);

        public static HttpClientHandler DefaultHandler = new HttpClientHandler
        {
            PreAuthenticate = true,
            AllowAutoRedirect = true,
            AutomaticDecompression =
                DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.None
        };

        public static HttpClientHandler PerRequestHandler = DefaultHandler;

        public static HttpClientHandler NtlmHandler = new HttpClientHandler
        {
            UseDefaultCredentials = true,
            PreAuthenticate = true,
            ClientCertificateOptions = ClientCertificateOption.Automatic
        };

        public Http()
        {
            Headers = new NameValueCollection();
        }

        public Uri Url { get; set; }

        public NameValueCollection Headers { get; private set; }

        public string RequestBody { get; set; }

        public string RequestContentType { get; set; }

        public HttpResponse Execute(Method method)
        {
            var request = BuildRequest(method);

            return GetResponse(request);
        }

        private static HttpResponse GetResponse(HttpRequestMessage request)
        {
            using (var client = ClientFactory())
            {
                var response = client.SendAsync(request).Result;

                var contentString = response.Content != null ? response.Content.ReadAsStringAsync().Result : null;
                
                var httpResponse = new HttpResponse
                {
                    StatusCode = response.StatusCode,
                    StatusDescription = response.ReasonPhrase,
                    Content = contentString,
                    ContentType = GetContentType(response)
                };

                foreach (var header in response.Headers)
                {
                    foreach (var headerValue in header.Value)
                    {
                        httpResponse.Headers.Add(header.Key, headerValue);
                    }
                }

                return httpResponse;
            }
        }

        private static string GetContentType(HttpResponseMessage response)
        {
            if (response.Content == null)
                return null;

            var contentType = response.Content.Headers.ContentType;
            if (contentType == null)
                return null;

            return contentType.MediaType;
        }

        private HttpRequestMessage BuildRequest(Method method)
        {
            var request = new HttpRequestMessage { RequestUri = Url };

            foreach (var name in Headers.AllKeys)
            {
                var value = Headers[name];
                request.Headers.Add(name, value);
            }

            if (string.IsNullOrEmpty(request.Headers.UserAgent.ToString()))
            {
                request.Headers.Add("User-Agent", "RestLib");
            }

            switch (method)
            {
                case Method.GET:
                    request.Method = HttpMethod.Get;
                    break;

                case Method.POST:
                    request.Method = HttpMethod.Post;
                    request.Content = new StringContent(RequestBody, Encoding.UTF8, RequestContentType);
                    break;

                default:
                    throw new NotImplementedException(string.Format("Method '{0}' not implemented.", method));
            }

            return request;
        }
    }
}