using System;

namespace CrispyWaffle.HttpClient.Serialization
{
    public interface IJsonSerializer
    {
        string Serialize<T>(T value);
        T? Deserialize<T>(string json);
        object? Deserialize(string json, Type type);
    }
}
