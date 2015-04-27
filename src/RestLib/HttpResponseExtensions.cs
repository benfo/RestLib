namespace RestLib
{
    public static class HttpResponseExtensions
    {
        public static IRestResponse ToRestResponse(this HttpResponse httpResponse)
        {
            return new RestResponse
            {
                StatusCode = httpResponse.StatusCode,
                StatusDescription = httpResponse.StatusDescription,
                Content = httpResponse.Content,
                ContentType = httpResponse.ContentType,
                Headers = httpResponse.Headers
            };
        }
    }
}