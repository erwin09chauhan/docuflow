using DocuFlow.Application.Abstractions.Services;
using Hangfire;

namespace DocuFlow.Infrastructure.Services;

public class BackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _jobClient;

    public BackgroundJobService(IBackgroundJobClient jobClient)
    {
        _jobClient = jobClient;
    }

    public string Enqueue(Guid documentId, Guid tenantId)
    {
        return _jobClient.Enqueue<IDocumentProcessingService>(
            s => s.ProcessAsync(documentId, tenantId, CancellationToken.None));
    }
}