using System.Security.Cryptography;

public static class FileHasher
{
    public static event EventHandler<double>? ProgressChanged;

    public static async Task<string> ComputeHashAsync(
        string path,
        HashAlgorithmType algorithm = HashAlgorithmType.SHA256,
        CancellationToken cancellationToken = default)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true);

        using HashAlgorithm hashAlgorithm = algorithm switch
        {
            HashAlgorithmType.SHA256 => SHA256.Create(),
            HashAlgorithmType.SHA1 => SHA1.Create(),
            HashAlgorithmType.MD5 => MD5.Create(),
            HashAlgorithmType.SHA384 => SHA384.Create(),
            HashAlgorithmType.SHA512 => SHA512.Create(),
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

            ProgressChanged?.Invoke(null, (double)totalRead / length * 100);
        }

        if (totalRead == length && length % buffer.Length == 0)
            hashAlgorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        return BitConverter.ToString(hashAlgorithm.Hash!).Replace("-", "").ToLowerInvariant();
    }
}