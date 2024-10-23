namespace CrispyWaffle.Sagas;

public class SagaData : ISagaData
{
    public Guid SagaId { get; set; }
    public string State { get; set; }
    public DateTime CreatedAt { get; set; }
    // Additional properties as needed

    public SagaData()
    {
        SagaId = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
}
