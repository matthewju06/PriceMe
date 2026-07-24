namespace PearlMetric.GatewayApi.Storage;

public sealed record StoredImage(
    string StorageKey,
    string ContentType,
    string Extension,
    long ByteSize,
    int Width,
    int Height);

public interface IImageStore
{
    Task<StoredImage> SaveAsync(
        Guid scanRunId,
        int sequenceIndex,
        Stream content,
        string? originalFileName,
        CancellationToken cancellationToken);

    Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken);
}
