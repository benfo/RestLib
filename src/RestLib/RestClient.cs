using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Xml.Serialization;

namespace RestLib
{
    public interface IRestClient
    {
        NameValueCollection Headers { get; }

        IRestResponse Get();

        IRestResponse<T> Get<T>();

        IRestResponse Get(string resourceName);

        IRestResponse<T> Get<T>(string resourceName);

        IRestResponse Get(string resourceName, string id);

        IRestResponse<T> Get<T>(string resourceName, string id);

        void AddHeader(string name, string value);
    }

    public class RestClient : IRestClient
    {
        public static Func<IHttp> HttpFactory = () => new Http();
        private readonly IHttp _http;
        private readonly string _baseAddress;

        public RestClient(string baseAddress)
        {
            ContentHandlers = new Dictionary<string, IDeserializer>
            {
                {"application/json", new NewtonsoftJsonDeserializer()},
                {"text/json", new NewtonsoftJsonDeserializer()},
                {"text/x-json", new NewtonsoftJsonDeserializer()},
                {"text/javascript", new NewtonsoftJsonDeserializer()},
                //{"application/xml", new DotNetXmlDeserializer()},
                //{"text/xml", new DotNetXmlDeserializer()},
                //{"*", new DotNetXmlDeserializer()}
            };

            _http = HttpFactory();
            _baseAddress = baseAddress;

            Headers = new NameValueCollection();
        }

        private Dictionary<string, IDeserializer> ContentHandlers { get; set; }

        public NameValueCollection Headers { get; private set; }

        public IRestResponse Get()
        {
            return PerformGet(_baseAddress);
        }

        public IRestResponse<T> Get<T>()
        {
            var response = Get();

            return MakeGenericResponse<T>(response);
        }

        public IRestResponse Get(string resourceName)
        {
            return PerformGet(string.Format("{0}/{1}", _baseAddress, resourceName));
        }

        public IRestResponse<T> Get<T>(string resourceName)
        {
            var response = Get(resourceName);

            return MakeGenericResponse<T>(response);
        }

        private IRestResponse<T> MakeGenericResponse<T>(IRestResponse response)
        {
            var newResponse = new RestResponse<T>
            {
                Content = response.Content,
                StatusCode = response.StatusCode,
                ContentType = response.ContentType
            };

            if (response.Content == null)
            {
                newResponse.Data = default(T);
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var deserializer = GetContentDeserializer(response.ContentType);
                    newResponse.Data = deserializer.Deserialize<T>(response.Content);
                }
                else
                {
                    newResponse.Data = default(T);
                }
            }

            return newResponse;
        }

        public IRestResponse Get(string resourceName, string id)
        {
            return PerformGet(string.Format("{0}/{1}/{2}", _baseAddress, resourceName, id));
        }

        public IRestResponse<T> Get<T>(string resourceName, string id)
        {
            var response = Get(resourceName, id);

            return MakeGenericResponse<T>(response);
        }

        public void AddHeader(string name, string value)
        {
            Headers.Add(name, value);
        }

        private IDeserializer GetContentDeserializer(string contentType)
        {
            if (contentType == null) throw new ArgumentNullException("contentType");

            IDeserializer handler = null;

            if (ContentHandlers.ContainsKey(contentType))
            {
                handler = ContentHandlers[contentType];
            }

            return handler;
        }

        private IRestResponse PerformGet(string url)
        {
            var httpResponse = _http.Request(url, Method.GET, Headers);

            return new RestResponse
            {
                StatusCode = httpResponse.StatusCode,
                Content = httpResponse.Content,
                ContentType = httpResponse.ContentType
            };
        }

        public IRestRequest Resource(string resourceName)
        {
            return new RestRequest(_baseAddress, resourceName, HttpFactory());
        }
    }

    public class RestRequest : IRestRequest
    {
        private readonly Uri _endPoint;
        private readonly string _resourceName;
        private readonly IHttp _http;

        public RestRequest(string endPoint, string resourceName, IHttp http)
        {
            Headers = new NameValueCollection();
            Parameters = new List<Parameter>();

            _endPoint = new Uri(endPoint);
            _resourceName = resourceName;
            _http = http;
        }

        public NameValueCollection Headers { get; private set; }

        public List<Parameter> Parameters { get; private set; }

        public IRestResponse Get()
        {
            var uri = BuildUri();
            return PerformGet(uri);
        }

        public IRestResponse<IEnumerable<T>> Get<T>()
        {
            throw new NotImplementedException();
        }

        public IRestResponse Get(string id)
        {
            var uri = BuildUri(id);
            return PerformGet(uri);
        }

        public IRestResponse<T> Get<T>(string id)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddMatrixParameter(string name, string value)
        {
            Parameters.Add(new Parameter(name, value, ParameterType.Matrix));
            return this;
        }

        private IRestResponse PerformGet(Uri uri)
        {
            var httpResponse = _http.Request(uri.ToString(), Method.GET, Headers);

            return new RestResponse
            {
                StatusCode = httpResponse.StatusCode,
                Content = httpResponse.Content,
                ContentType = httpResponse.ContentType
            };
        }

        private Uri BuildUri(string resourceIdentifier = null)
        {
            var builder = new UriBuilder(_endPoint)
            {
                Path = PathCombine(_resourceName, resourceIdentifier)
            };

            // Don't include port 80/443 in the Uri.
            if (builder.Uri.IsDefaultPort)
            {
                builder.Port = -1;
            }

            return builder.Uri;
        }

        private static string PathCombine(string path1, string path2)
        {
            if (string.IsNullOrWhiteSpace(path1))
            {
                return path2;
            }

            if (string.IsNullOrWhiteSpace(path2))
            {
                return path1;
            }

            path1 = path1.TrimEnd('/', '\\');
            path2 = path2.TrimStart('/', '\\');

            return string.Format("{0}/{1}", path1, path2);
        }

        public IRestRequest AddHeader(string name, string value)
        {
            Headers.Add(name, value);
            return this;
        }

        public IRestRequest AddQuery(string name, string value)
        {
            Parameters.Add(new Parameter(name, value, ParameterType.Query));
            return this;
        }
    }

    public enum ParameterType
    {
        Query,
        Matrix
    }

    public class Parameter
    {
        public Parameter(string name, string value, ParameterType type)
        {
            Name = name;
            Value = value;
            Type = type;
        }

        public string Name { get; private set; }

        public string Value { get; private set; }

        public ParameterType Type { get; private set; }
    }

    public interface IRestRequest
    {
        IRestResponse Get();

        IRestResponse<IEnumerable<T>> Get<T>();

        IRestResponse Get(string id);

        IRestResponse<T> Get<T>(string id);

        IRestRequest AddMatrixParameter(string name, string value);
    }

    public interface IDeserializer
    {
        T Deserialize<T>(string content);
    }

    public class NewtonsoftJsonDeserializer : IDeserializer
    {
        public T Deserialize<T>(string content)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(content);
        }
    }

    public class DotNetXmlDeserializer : IDeserializer
    {
        public T Deserialize<T>(string content)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var writer = new StringReader(content))
            {
                return (T)serializer.Deserialize(writer);
            }
        }
    }

    public interface IRestResponse
    {
        HttpStatusCode StatusCode { get; set; }

        string Content { get; set; }

        string ContentType { get; set; }
    }

    public interface IRestResponse<T> : IRestResponse
    {
        T Data { get; set; }
    }

    public class RestResponse : IRestResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public string Content { get; set; }

        public string ContentType { get; set; }
    }

    public class RestResponse<T> : RestResponse, IRestResponse<T>
    {
        public T Data { get; set; }
    }

    public interface IHttp
    {
        HttpResponse Request(string url, Method method, NameValueCollection headers);
    }

    public enum Method
    {
        GET,
        POST
    }

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

        public HttpResponse Request(string url, Method method, NameValueCollection headers)
        {
            var request = BuildRequest(url, method, headers);

            return GetResponse(request);
        }

        private static HttpResponse GetResponse(HttpRequestMessage request)
        {
            using (var client = ClientFactory())
            {
                var response = client.SendAsync(request).Result;

                var contentString = response.Content != null ? response.Content.ReadAsStringAsync().Result : null;

                return new HttpResponse
                {
                    StatusCode = response.StatusCode,
                    Content = contentString,
                    ContentType = GetContentType(response)
                };
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

        private static HttpRequestMessage BuildRequest(string url, Method method, NameValueCollection headers)
        {
            var request = new HttpRequestMessage { RequestUri = new Uri(url) };

            foreach (var name in headers.AllKeys)
            {
                var value = headers[name];
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
                //case "POST":
                //    request.Method = HttpMethod.Post;
                //    break;
                default:
                    throw new NotImplementedException(string.Format("Method '{0}' not implemented.", method));
            }

            return request;
        }
    }

    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public string Content { get; set; }

        public string ContentType { get; set; }
    }
}