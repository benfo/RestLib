using System.Collections.Specialized;

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
}