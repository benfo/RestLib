using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Serialization;

namespace RestLib
{
    public interface IRestClient
    {
        NameValueCollection Headers { get; }

        IRestResponse Get();

        IRestResponse<T> Get<T>();

        void AddHeader(string name, string value);

        IRestRequest Resource(string resourceName);
    }

    public class RestClient : IRestClient
    {
        public static Func<IHttp> HttpFactory = () => new Http();
        private readonly IHttp _http;
        private readonly string _endPoint;

        public RestClient(string endPoint)
        {
            _http = HttpFactory();
            _endPoint = endPoint;

            Headers = new NameValueCollection();
        }

        public NameValueCollection Headers { get; private set; }

        public IRestResponse Get()
        {
            var request = BuildRequest();
            return request.Get();
        }

        public IRestResponse<T> Get<T>()
        {
            var response = Get();
            return response.ToGenericResponse<T>();
        }

        public void AddHeader(string name, string value)
        {
            Headers.Add(name, value);
        }

        public IRestRequest Resource(string resourceName)
        {
            return BuildRequest(resourceName);
        }

        private RestRequest BuildRequest(string resourceName = null)
        {
            var request = new RestRequest(_endPoint, resourceName, _http);
            foreach (var name in Headers.AllKeys)
            {
                request.AddHeader(name, Headers[name]);
            }
            return request;
        }
    }

    public static class ContentHandlerProvider
    {
        static ContentHandlerProvider()
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
        }

        public static Dictionary<string, IDeserializer> ContentHandlers { get; set; }

        public static IDeserializer GetContentDeserializer(string contentType)
        {
            if (contentType == null) throw new ArgumentNullException("contentType");

            IDeserializer handler = null;

            if (ContentHandlers.ContainsKey(contentType))
            {
                handler = ContentHandlers[contentType];
            }

            return handler;
        }
    }

    public class RestRequest : IRestRequest
    {
        private const string QueryStringParameterDelimiter = "&";
        private const string MatrixParameterDelimiter = ";";
        private bool OmitEmptyParameters = true;

        private readonly Uri _endPoint;
        private readonly string _resourceName;
        private readonly IHttp _http;

        public RestRequest(string endPoint, string resourceName, IHttp http)
        {
            Headers = new NameValueCollection();
            Parameters = new List<Parameter>();
            JsonSerializer = new NewtonsoftJsonSerializer();

            _endPoint = new Uri(endPoint);
            _resourceName = resourceName;
            _http = http;
        }

        public NameValueCollection Headers { get; private set; }

        public List<Parameter> Parameters { get; private set; }

        public ISerializer JsonSerializer { get; set; }

        public IRestResponse Post(object obj)
        {
            var uri = BuildUri();
            var body = JsonSerializer.Serialize(obj);
            Parameters.Add(new Parameter(JsonSerializer.ContentType, body, ParameterType.RequestBody));
            
            return PerformPost(uri);
        }

        public IRestResponse<T> Post<T>(T obj)
        {
            var response = Post((object)obj);

            return response.ToGenericResponse<T>();
        }

        public IRestResponse Get()
        {
            var uri = BuildUri();
            return PerformGet(uri);
        }

        public IRestResponse<T> Get<T>()
        {
            var response = Get();
            return response.ToGenericResponse<T>();
        }

        public IRestResponse Get(string id)
        {
            var uri = BuildUri(id);
            return PerformGet(uri);
        }

        public IRestResponse<T> Get<T>(string id)
        {
            var response = Get(id);
            return response.ToGenericResponse<T>();
        }

        public IRestRequest AddMatrixParameter(string name, string value)
        {
            Parameters.Add(new Parameter(name, value, ParameterType.Matrix));
            return this;
        }

        public IRestRequest AddHeader(string name, string value)
        {
            Headers.Add(name, value);
            return this;
        }

        public IRestRequest AddQueryParameter(string name, string value)
        {
            Parameters.Add(new Parameter(name, value, ParameterType.QueryString));
            return this;
        }

        private IRestResponse PerformGet(Uri uri)
        {
            ConfigureHttp(uri);

            var httpResponse = _http.Execute(Method.GET);

            return new RestResponse
            {
                StatusCode = httpResponse.StatusCode,
                StatusDescription = httpResponse.StatusDescription,
                Content = httpResponse.Content,
                ContentType = httpResponse.ContentType,
                Headers = httpResponse.Headers
            };
        }

        private IRestResponse PerformPost(Uri uri)
        {
            ConfigureHttp(uri);
            
            var httpResponse = _http.Execute(Method.POST);
            
            return new RestResponse
            {
                StatusCode = httpResponse.StatusCode,
                StatusDescription = httpResponse.StatusDescription,
                Content = httpResponse.Content,
                ContentType = httpResponse.ContentType,
                Headers = httpResponse.Headers
            };
        }

        private void ConfigureHttp(Uri uri)
        {
            _http.Url = uri;
            foreach (var headerName in Headers.AllKeys)
            {
                _http.Headers.Add(headerName, Headers[headerName]);
            }

            var bodyParm = Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
            if (bodyParm != null)
            {
                _http.RequestBody = bodyParm.Value;
                _http.RequestContentType = bodyParm.Name;
            }
        }

        private Uri BuildUri(string resourceIdentifier = null)
        {
            var builder = new UriBuilder(_endPoint);
            builder.Path = PathCombine(PathCombine(builder.Path, _resourceName), resourceIdentifier);

            // Don't include port 80/443 in the Uri.
            if (builder.Uri.IsDefaultPort)
            {
                builder.Port = -1;
            }

            var queryStringParms = Parameters
                .Where(p => p.Type == ParameterType.QueryString && (!OmitEmptyParameters || !string.IsNullOrWhiteSpace(p.Value)))
                .ToList();
            if (queryStringParms.Any())
            {
                builder.Query = EncodeParameters(queryStringParms, QueryStringParameterDelimiter);
            }

            var matrixParams = Parameters
                .Where(p => p.Type == ParameterType.Matrix && (!OmitEmptyParameters || !string.IsNullOrWhiteSpace(p.Value)))
                .ToList();
            if (matrixParams.Any())
            {
                var encodedParameters = EncodeParameters(matrixParams, MatrixParameterDelimiter);
                builder.Path = string.Concat(builder.Path, MatrixParameterDelimiter, encodedParameters);
            }

            return builder.Uri;
        }

        private static string EncodeParameters(IEnumerable<Parameter> parameters, string delimiter)
        {
            return string.Join(delimiter, parameters.Select(EncodeParameter).ToArray());
        }

        private static string EncodeParameter(Parameter parameter)
        {
            return parameter.Value == null
                ? string.Concat(parameter.Name.UrlEncode(), "=")
                : string.Concat(parameter.Name.UrlEncode(), "=", parameter.Value.UrlEncode());
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
    }

    public static class RestResponseExtensions
    {
        public static IRestResponse<T> ToGenericResponse<T>(this IRestResponse response)
        {
            var newResponse = new RestResponse<T>
            {
                Content = response.Content,
                StatusCode = response.StatusCode,
                StatusDescription = response.StatusDescription,
                ContentType = response.ContentType,
                Headers = response.Headers
            };

            if (response.Content == null)
            {
                newResponse.Data = default(T);
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                {
                    var deserializer = ContentHandlerProvider.GetContentDeserializer(response.ContentType);
                    newResponse.Data = deserializer.Deserialize<T>(response.Content);
                }
                else
                {
                    newResponse.Data = default(T);
                }
            }

            return newResponse;
        }
    }

    public static class StringExtensions
    {
        /// <summary>
        /// Uses Uri.EscapeDataString() based on recommendations on MSDN
        /// http://blogs.msdn.com/b/yangxind/archive/2006/11/09/don-t-use-net-system-uri-unescapedatastring-in-url-decoding.aspx
        /// </summary>
        public static string UrlEncode(this string input)
        {
            const int maxLength = 32766;
            if (input == null)
                throw new ArgumentNullException("input");

            if (input.Length <= maxLength)
                return Uri.EscapeDataString(input);

            StringBuilder sb = new StringBuilder(input.Length * 2);
            int index = 0;

            while (index < input.Length)
            {
                int length = Math.Min(input.Length - index, maxLength);
                string subString = input.Substring(index, length);

                sb.Append(Uri.EscapeDataString(subString));
                index += subString.Length;
            }

            return sb.ToString();
        }
    }

    public enum ParameterType
    {
        QueryString,
        Matrix,
        RequestBody
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

        IRestResponse<T> Get<T>();

        IRestResponse Get(string id);

        IRestResponse<T> Get<T>(string id);

        IRestRequest AddMatrixParameter(string name, string value);

        IRestRequest AddQueryParameter(string name, string value);

        IRestRequest AddHeader(string name, string value);

        NameValueCollection Headers { get; }

        List<Parameter> Parameters { get; }

        IRestResponse Post(object obj);
        
        IRestResponse<T> Post<T>(T obj);

        ISerializer JsonSerializer { get; set; }
    }

    public interface IDeserializer
    {
        T Deserialize<T>(string content);
    }

    public interface ISerializer
    {
        string Serialize(object obj);

        string ContentType { get; set; }
    }

    public class NewtonsoftJsonDeserializer : IDeserializer
    {
        public T Deserialize<T>(string content)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(content);
        }
    }

    public class NewtonsoftJsonSerializer : ISerializer
    {
        public NewtonsoftJsonSerializer()
        {
            ContentType = "application/json";
        }

        public string Serialize(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        public string ContentType { get; set; }
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

        string StatusDescription { get; set; }

        string Content { get; set; }

        string ContentType { get; set; }

        NameValueCollection Headers { get; }
    }

    public interface IRestResponse<T> : IRestResponse
    {
        T Data { get; set; }
    }

    public abstract class RestResponseBase
    {
        protected RestResponseBase()
        {
            Headers = new NameValueCollection();
        }

        public HttpStatusCode StatusCode { get; set; }

        public string StatusDescription { get; set; }
        
        public string Content { get; set; }

        public string ContentType { get; set; }
       
        public NameValueCollection Headers { get; protected internal set; }
    }

    public class RestResponse : RestResponseBase, IRestResponse { }

    public class RestResponse<T> : RestResponseBase, IRestResponse<T>
    {
        public T Data { get; set; }
    }

    public interface IHttp
    {
        Uri Url { get; set; }

        NameValueCollection Headers { get; }

        string RequestBody { get; set; }

        string RequestContentType { get; set; }

        HttpResponse Execute(Method method);
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

    public class HttpResponse
    {
        public HttpResponse()
        {
            Headers = new NameValueCollection();
        }

        public HttpStatusCode StatusCode { get; set; }

        public string StatusDescription { get; set; }
        
        public string Content { get; set; }

        public string ContentType { get; set; }
        
        public NameValueCollection Headers { get; private set; }
    }
}