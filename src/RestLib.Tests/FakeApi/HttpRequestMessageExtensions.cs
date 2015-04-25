using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace RestLib.Tests.FakeApi
{
    public static class HttpRequestMessageExtensions
    {
        public static string GetHeader(this HttpRequestMessage request, string key)
        {
            IEnumerable<string> keys = null;
            if (!request.Headers.TryGetValues(key, out keys))
                return null;

            return keys.First();
        } 
    }
}