namespace PearlMetric.GatewayApi.Storage;

public static class ImageDimensionReader
{
    public static (int Width, int Height) Read(Stream stream, string extension)
    {
        if (!stream.CanSeek)
        {
            throw new ImageValidationException("Image stream must be seekable for dimension checks.");
        }

        var origin = stream.Position;
        try
        {
            return extension.ToLowerInvariant() switch
            {
                "jpg" or "jpeg" => ReadJpeg(stream),
                "png" => ReadPng(stream),
                "webp" => ReadWebp(stream),
                _ => throw new ImageValidationException($"Unsupported extension '{extension}'.")
            };
        }
        finally
        {
            stream.Position = origin;
        }
    }

    private static (int Width, int Height) ReadPng(Stream stream)
    {
        stream.Position = 16;
        Span<byte> buffer = stackalloc byte[8];
        if (stream.Read(buffer) != 8)
        {
            throw new ImageValidationException("PNG header is truncated.");
        }

        var width = ReadInt32BigEndian(buffer[..4]);
        var height = ReadInt32BigEndian(buffer[4..]);
        return (width, height);
    }

    private static (int Width, int Height) ReadJpeg(Stream stream)
    {
        stream.Position = 0;
        if (stream.ReadByte() != 0xFF || stream.ReadByte() != 0xD8)
        {
            throw new ImageValidationException("Invalid JPEG header.");
        }

        Span<byte> lengthBytes = stackalloc byte[2];
        Span<byte> sofPayload = stackalloc byte[7];

        while (true)
        {
            var markerPrefix = stream.ReadByte();
            if (markerPrefix == -1)
            {
                throw new ImageValidationException("JPEG ended before dimensions were found.");
            }

            if (markerPrefix != 0xFF)
            {
                continue;
            }

            int marker;
            do
            {
                marker = stream.ReadByte();
            }
            while (marker == 0xFF);

            if (marker == -1)
            {
                throw new ImageValidationException("JPEG ended before dimensions were found.");
            }

            // SOF0..SOF3, SOF5..SOF7, SOF9..SOF11, SOF13..SOF15
            if ((marker >= 0xC0 && marker <= 0xC3)
                || (marker >= 0xC5 && marker <= 0xC7)
                || (marker >= 0xC9 && marker <= 0xCB)
                || (marker >= 0xCD && marker <= 0xCF))
            {
                if (stream.Read(sofPayload) != 7)
                {
                    throw new ImageValidationException("JPEG SOF segment is truncated.");
                }

                var height = (sofPayload[3] << 8) | sofPayload[4];
                var width = (sofPayload[5] << 8) | sofPayload[6];
                return (width, height);
            }

            if (marker == 0xD9 || marker == 0xDA)
            {
                throw new ImageValidationException("JPEG dimensions were not found.");
            }

            if (stream.Read(lengthBytes) != 2)
            {
                throw new ImageValidationException("JPEG segment length is truncated.");
            }

            var length = (lengthBytes[0] << 8) | lengthBytes[1];
            if (length < 2)
            {
                throw new ImageValidationException("JPEG segment length is invalid.");
            }

            stream.Position += length - 2;
        }
    }

    private static (int Width, int Height) ReadWebp(Stream stream)
    {
        stream.Position = 12;
        Span<byte> chunkHeader = stackalloc byte[8];
        if (stream.Read(chunkHeader) != 8)
        {
            throw new ImageValidationException("WebP header is truncated.");
        }

        var fourCc = System.Text.Encoding.ASCII.GetString(chunkHeader[..4]);
        if (fourCc == "VP8X")
        {
            Span<byte> payload = stackalloc byte[10];
            if (stream.Read(payload) != 10)
            {
                throw new ImageValidationException("WebP VP8X chunk is truncated.");
            }

            var width = 1 + (payload[4] | (payload[5] << 8) | (payload[6] << 16));
            var height = 1 + (payload[7] | (payload[8] << 8) | (payload[9] << 16));
            return (width, height);
        }

        if (fourCc == "VP8 ")
        {
            Span<byte> payload = stackalloc byte[10];
            if (stream.Read(payload) != 10)
            {
                throw new ImageValidationException("WebP VP8 chunk is truncated.");
            }

            var width = payload[6] | ((payload[7] & 0x3F) << 8);
            var height = payload[8] | ((payload[9] & 0x3F) << 8);
            return (width, height);
        }

        if (fourCc == "VP8L")
        {
            Span<byte> payload = stackalloc byte[5];
            if (stream.Read(payload) != 5)
            {
                throw new ImageValidationException("WebP VP8L chunk is truncated.");
            }

            if (payload[0] != 0x2F)
            {
                throw new ImageValidationException("Invalid lossless WebP signature.");
            }

            var bits = payload[1] | (payload[2] << 8) | (payload[3] << 16) | (payload[4] << 24);
            var width = (bits & 0x3FFF) + 1;
            var height = ((bits >> 14) & 0x3FFF) + 1;
            return (width, height);
        }

        throw new ImageValidationException("Unsupported WebP encoding.");
    }

    private static int ReadInt32BigEndian(ReadOnlySpan<byte> bytes) =>
        (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
}
