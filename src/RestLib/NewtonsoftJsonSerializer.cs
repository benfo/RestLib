namespace RestLib
{
    public class NewtonsoftJsonSerializer : ISerializer
    {
        public NewtonsoftJsonSerializer()
        {
            ContentType = "application/json";
        }

        public string Serialize(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        public string ContentType { get; set; }
    }
}