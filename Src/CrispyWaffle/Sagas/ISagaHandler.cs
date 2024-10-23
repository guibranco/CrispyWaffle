namespace CrispyWaffle.Sagas;

public interface ISagaHandler
{
    void Handle<T>(T message);
    void HandleTimeout<T>(T message);
}
