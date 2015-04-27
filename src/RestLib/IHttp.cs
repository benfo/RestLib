using System;
using System.Collections.Specialized;

namespace RestLib
{
    public interface IHttp
    {
        Uri Url { get; set; }

        NameValueCollection Headers { get; }

        string RequestBody { get; set; }

        string RequestContentType { get; set; }

        HttpResponse Execute(Method method);
    }
}