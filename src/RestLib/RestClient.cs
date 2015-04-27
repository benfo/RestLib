using System;
using System.Collections.Specialized;

namespace RestLib
{
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
}