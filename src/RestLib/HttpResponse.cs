using System.Collections.Specialized;
using System.Net;

namespace RestLib
{
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