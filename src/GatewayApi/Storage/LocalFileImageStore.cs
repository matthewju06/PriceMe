using Microsoft.Extensions.Options;
using PearlMetric.GatewayApi.Configuration;
using PearlMetric.GatewayApi.Contracts.CvWorker;

namespace PearlMetric.GatewayApi.Storage;

public sealed class LocalFileImageStore(IOptions<ImageStorageOptions> options) : IImageStore
{
    private readonly ImageStorageOptions _options = options.Value;

    public async Task<StoredImage> SaveAsync(
        Guid scanRunId,
        int sequenceIndex,
        Stream content,
        string? originalFileName,
        CancellationToken cancellationToken)
    {
        if (sequenceIndex < 0 || sequenceIndex >= _options.MaxFramesPerRun)
        {
            throw new ImageValidationException(
                $"SequenceIndex must be between 0 and {_options.MaxFramesPerRun - 1}.");
        }

        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);

        if (buffer.Length <= 0)
        {
            throw new ImageValidationException("Uploaded image is empty.");
        }

        if (buffer.Length > _options.MaxFrameBytes)
        {
            throw new ImageValidationException(
                $"Image exceeds the maximum size of {_options.MaxFrameBytes} bytes.");
        }

        buffer.Position = 0;
        Span<byte> header = stackalloc byte[16];
        var read = buffer.Read(header);
        buffer.Position = 0;

        var format = ImageFormatDetector.Detect(header[..read]);
        var (width, height) = ImageDimensionReader.Read(buffer, format.Extension);

        if (width < _options.MinWidth || height < _options.MinHeight)
        {
            throw new ImageValidationException(
                $"Image is too small. Minimum size is {_options.MinWidth}x{_options.MinHeight}.");
        }

        if (width > _options.MaxWidth || height > _options.MaxHeight)
        {
            throw new ImageValidationException(
                $"Image is too large. Maximum size is {_options.MaxWidth}x{_options.MaxHeight}.");
        }

        var storageKey = $"runs/{scanRunId}/{sequenceIndex}.{format.Extension}";
        if (!CvWorkerProtocol.IsValidStorageKey(storageKey))
        {
            throw new ImageValidationException("Generated storage key is invalid.");
        }

        var absolutePath = GetAbsolutePath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        buffer.Position = 0;
        await using (var file = File.Create(absolutePath))
        {
            await buffer.CopyToAsync(file, cancellationToken);
        }

        return new StoredImage(
            storageKey,
            format.ContentType,
            format.Extension,
            buffer.Length,
            width,
            height);
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!CvWorkerProtocol.IsValidStorageKey(storageKey))
        {
            return Task.FromResult<Stream?>(null);
        }

        var absolutePath = GetAbsolutePath(storageKey);
        if (!File.Exists(absolutePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(
            absolutePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!CvWorkerProtocol.IsValidStorageKey(storageKey))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(File.Exists(GetAbsolutePath(storageKey)));
    }

    private string GetAbsolutePath(string storageKey)
    {
        var root = Path.GetFullPath(_options.RootPath);
        var combined = Path.GetFullPath(Path.Combine(root, storageKey.Replace('/', Path.DirectorySeparatorChar)));

        if (!combined.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && !string.Equals(combined, root, StringComparison.Ordinal))
        {
            throw new ImageValidationException("Storage key resolves outside the image root.");
        }

        return combined;
    }
}
