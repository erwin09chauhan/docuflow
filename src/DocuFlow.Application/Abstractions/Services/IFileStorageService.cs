namespace DocuFlow.Application.Abstractions.Services;

public record FileUploadResult(
    string FilePath,
    string FileName,
    long SizeBytes
);

public interface IFileStorageService
{
    Task<FileUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}