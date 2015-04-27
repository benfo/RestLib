using System.Net;

namespace RestLib
{
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
}