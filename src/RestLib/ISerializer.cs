namespace RestLib
{
    public interface ISerializer
    {
        string Serialize(object obj);

        string ContentType { get; set; }
    }
}