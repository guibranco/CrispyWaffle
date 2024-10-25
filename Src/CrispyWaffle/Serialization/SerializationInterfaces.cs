namespace CrispyWaffle.Serialization
{
    public interface ISerializer
    {
        string Serialize<T>(T obj);
    }

    public interface IDeserializer
    {
        T Deserialize<T>(string yaml);
    }
}
