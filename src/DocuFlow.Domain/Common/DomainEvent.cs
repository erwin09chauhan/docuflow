namespace DocuFlow.Domain.Common;

public abstract class DomainEvent
{
    public Guid Id { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public bool IsPublished { get; set; }

    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
    }
}