namespace CrispyWaffle.Sagas;

public abstract class GenericSagaHandler : ISagaHandler
{
    public abstract void Handle<T>(T message);

    public abstract void HandleTimeout<T>(T message);

    protected void StartSaga<T>(T message)
    {
        // Logic to start a saga
    }

    protected void UpdateSagaState<T>(T message)
    {
        // Logic to update saga state
    }

    protected void CompleteSaga()
    {
        // Logic to complete a saga
    }
}
