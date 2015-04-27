namespace RestLib
{
    public interface IDeserializer
    {
        T Deserialize<T>(string content);
    }
}