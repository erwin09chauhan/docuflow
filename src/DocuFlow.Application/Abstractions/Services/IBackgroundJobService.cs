namespace DocuFlow.Application.Abstractions.Services;

public interface IBackgroundJobService
{
    string Enqueue(Guid documentId, Guid tenantId);
}