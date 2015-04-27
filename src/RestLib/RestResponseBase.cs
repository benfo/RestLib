using System.Collections.Specialized;
using System.Net;

namespace RestLib
{
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
}