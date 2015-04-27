using System.Collections.Generic;
using System.Collections.Specialized;

namespace RestLib
{
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
}