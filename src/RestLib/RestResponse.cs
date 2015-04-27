namespace RestLib
{
    public class RestResponse<T> : RestResponseBase, IRestResponse<T>
    {
        public T Data { get; set; }
    }

    public class RestResponse : RestResponseBase, IRestResponse { }
}