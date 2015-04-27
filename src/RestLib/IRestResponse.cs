using System.Collections.Specialized;
using System.Net;

namespace RestLib
{
    public interface IRestResponse<T> : IRestResponse
    {
        T Data { get; set; }
    }

    public interface IRestResponse
    {
        HttpStatusCode StatusCode { get; set; }

        string StatusDescription { get; set; }

        string Content { get; set; }

        string ContentType { get; set; }

        NameValueCollection Headers { get; }
    }
}