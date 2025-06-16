using System.Security.Cryptography;

public static class FileHasher
{
    public static async Task<string> ComputeHashAsync(
        string path,
        string algorithm = "SHA256",
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true);

        using HashAlgorithm hashAlgorithm = algorithm.ToUpperInvariant() switch
        {
            "SHA256" => SHA256.Create(),
            "SHA1" => SHA1.Create(),
            "MD5" => MD5.Create(),
            "SHA384" => SHA384.Create(),
            "SHA512" => SHA512.Create(),
            _ => throw new ArgumentException("Unsupported algorithm", nameof(algorithm))
        };

        byte[] buffer = new byte[8192];
        long totalRead = 0;
        long length = stream.Length;
        int read;

        while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (read < buffer.Length)
                hashAlgorithm.TransformFinalBlock(buffer, 0, read);
            else
                hashAlgorithm.TransformBlock(buffer, 0, read, null, 0);

            totalRead += read;
            progress?.Report((double)totalRead / length * 100);
        }

        if (totalRead == length && length % buffer.Length == 0)
            hashAlgorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        return BitConverter.ToString(hashAlgorithm.Hash!).Replace("-", "").ToLowerInvariant();
    }
}