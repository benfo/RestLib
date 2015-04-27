using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace RestLib
{
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
            
            return Execute(uri, Method.POST);
        }

        public IRestResponse<T> Post<T>(T obj)
        {
            var response = Post((object)obj);

            return response.ToGenericResponse<T>();
        }

        public IRestResponse Get()
        {
            var uri = BuildUri();
            return Execute(uri, Method.GET);
        }

        public IRestResponse<T> Get<T>()
        {
            var response = Get();
            return response.ToGenericResponse<T>();
        }

        public IRestResponse Get(string id)
        {
            var uri = BuildUri(id);
            return Execute(uri, Method.GET);
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

        private IRestResponse Execute(Uri uri, Method method)
        {
            ConfigureHttp(uri);

            var httpResponse = _http.Execute(method);

            return httpResponse.ToRestResponse();
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
}