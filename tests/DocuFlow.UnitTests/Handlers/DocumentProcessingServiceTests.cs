using DocuFlow.Application.Abstractions.Repositories;
using DocuFlow.Application.Abstractions.Services;
using DocuFlow.Domain.Entities;
using DocuFlow.Domain.Enums;
using DocuFlow.Infrastructure.Services;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace DocuFlow.UnitTests.Handlers;

public class DocumentProcessingServiceTests
{
    private readonly Mock<IDocumentRepository> _documentRepoMock;
    private readonly Mock<IExtractionJobRepository> _extractionJobRepoMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IAiExtractionService> _aiExtractionMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IPublisher> _publisherMock;
    private readonly Mock<ILogger<DocumentProcessingService>> _loggerMock;
    private readonly DocumentProcessingService _service;

    public DocumentProcessingServiceTests()
    {
        _documentRepoMock = new Mock<IDocumentRepository>();
        _extractionJobRepoMock = new Mock<IExtractionJobRepository>();
        _fileStorageMock = new Mock<IFileStorageService>();
        _aiExtractionMock = new Mock<IAiExtractionService>();
        _emailServiceMock = new Mock<IEmailService>();
        _publisherMock = new Mock<IPublisher>();
        _loggerMock = new Mock<ILogger<DocumentProcessingService>>();

        _service = new DocumentProcessingService(
            _documentRepoMock.Object,
            _extractionJobRepoMock.Object,
            _fileStorageMock.Object,
            _aiExtractionMock.Object,
            _emailServiceMock.Object,
            _publisherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessAsync_DocumentNotFound_ReturnsWithoutProcessing()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        _documentRepoMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        // Act
        await _service.ProcessAsync(documentId, tenantId);

        // Assert — file storage should never be called if document not found
        _fileStorageMock.Verify(
            x => x.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_SuccessfulExtraction_SetsStatusToCompleted()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var document = Document.Create(tenantId, Guid.NewGuid(), "invoice.pdf",
                    "uploads/invoice.pdf", 1024, "application/pdf", DocuFlow.Domain.Enums.ExtractionSchema.Invoice);
        document.UpdateStatus(DocumentStatus.Queued);

        var job = ExtractionJob.Create(document.Id, tenantId, DocuFlow.Domain.Enums.ExtractionSchema.Invoice);

        _documentRepoMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _documentRepoMock
            .Setup(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _fileStorageMock
            .Setup(x => x.DownloadAsync(document.FilePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("file content")));

        _aiExtractionMock
                    .Setup(x => x.ExtractAsync(It.IsAny<ExtractionRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ExtractionServiceResult(true, new List<ExtractedFieldResult>
                    {
                new ExtractedFieldResult("InvoiceNumber", "INV-001", 0.99)
                    }));

        _extractionJobRepoMock
            .Setup(x => x.GetLatestByDocumentIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        _extractionJobRepoMock
            .Setup(x => x.UpdateAsync(It.IsAny<ExtractionJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessAsync(documentId, tenantId);

        // Assert
        document.Status.Should().Be(DocumentStatus.Completed);
        _publisherMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_AiExtractionFails_SetsStatusToFailed()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var document = Document.Create(tenantId, Guid.NewGuid(), "invoice.pdf",
                    "uploads/invoice.pdf", 1024, "application/pdf", DocuFlow.Domain.Enums.ExtractionSchema.Invoice);
        document.UpdateStatus(DocumentStatus.Queued);

        _documentRepoMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _documentRepoMock
            .Setup(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _fileStorageMock
            .Setup(x => x.DownloadAsync(document.FilePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("file content")));

        _aiExtractionMock
                    .Setup(x => x.ExtractAsync(It.IsAny<ExtractionRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ExtractionServiceResult(false, new List<ExtractedFieldResult>(), "AI service unavailable"));

        _extractionJobRepoMock
            .Setup(x => x.GetLatestByDocumentIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExtractionJob?)null);

        // Act
        var act = async () => await _service.ProcessAsync(documentId, tenantId);

        // Assert — service rethrows so Hangfire can retry
        await act.Should().ThrowAsync<Exception>().WithMessage("AI service unavailable");
        document.Status.Should().Be(DocumentStatus.Failed);
    }
}