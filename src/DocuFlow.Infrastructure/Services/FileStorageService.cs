using DocuFlow.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;

namespace DocuFlow.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public FileStorageService(IConfiguration configuration)
    {
        _basePath = configuration["FileStorage:BasePath"] ?? "uploads";
        Directory.CreateDirectory(_basePath);
    }

    public async Task<FileUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_basePath, uniqueFileName);

        await using (var fileOutput = File.Create(filePath))
        {
            await fileStream.CopyToAsync(fileOutput, cancellationToken);
            await fileOutput.FlushAsync(cancellationToken);
        }

        var sizeBytes = new FileInfo(filePath).Length;
        return new FileUploadResult(filePath, uniqueFileName, sizeBytes);
    }

    public async Task<Stream> DownloadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var memoryStream = new MemoryStream();
        await using var fileStream = File.OpenRead(filePath);
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }
}