using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DocuFlow.Infrastructure.Services;

public class MockAiExtractionService : IAiExtractionService
{
    private readonly ILogger<MockAiExtractionService> _logger;

    public MockAiExtractionService(ILogger<MockAiExtractionService> logger)
    {
        _logger = logger;
    }

    public Task<ExtractionServiceResult> ExtractAsync(
        ExtractionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MockAiExtractionService extracting for schema {Schema}", request.Schema);

        var fields = request.Schema switch
        {
            ExtractionSchema.Invoice => new List<ExtractedFieldResult>
            {
                new("InvoiceNumber", "INV-2024-001", 0.99),
                new("InvoiceDate", "2024-01-15", 0.98),
                new("TotalAmount", "1500.00", 0.97),
                new("VendorName", "Acme Corp", 0.96),
                new("DueDate", "2024-02-15", 0.95)
            },
            ExtractionSchema.Contract => new List<ExtractedFieldResult>
            {
                new("PartyOne", "DocuFlow Ltd", 0.99),
                new("PartyTwo", "Client Corp", 0.98),
                new("StartDate", "2024-01-01", 0.97),
                new("EndDate", "2024-12-31", 0.96),
                new("ContractValue", "50000.00", 0.95)
            },
            _ => new List<ExtractedFieldResult>
            {
                new("Field1", "MockValue1", 0.90),
                new("Field2", "MockValue2", 0.89)
            }
        };

        return Task.FromResult(new ExtractionServiceResult(true, fields));
    }
}