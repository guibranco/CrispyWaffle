namespace CrispyWaffle.Sagas;

public class SagaRepository
{
    private readonly Dictionary<Guid, SagaData> _storage = new Dictionary<Guid, SagaData>();

    public void Save(SagaData sagaData)
    {
        _storage[sagaData.SagaId] = sagaData;
    }

    public SagaData Load(Guid sagaId)
    {
        _storage.TryGetValue(sagaId, out var sagaData);
        return sagaData;
    }

    public void Delete(Guid sagaId)
    {
        _storage.Remove(sagaId);
    }
}
