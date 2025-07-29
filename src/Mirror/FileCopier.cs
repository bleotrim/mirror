public class FileCopier
{
    public event EventHandler<double>? CopyProgressChanged;
    public event EventHandler<HashProgressEventArgs>? HashProgressChanged;
    public event EventHandler<string>? StatusMessage;

    public async Task<bool> CopyWithVerificationAsync(string sourcePath, string destinationPath, FileCopyOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new FileCopyOptions();

        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("Source file does not exist.", sourcePath);

        if (File.Exists(destinationPath) && !options.Overwrite)
            throw new IOException("Destination file exists and overwrite is false.");

        EventHandler<double>? internalHandler = null;

        StatusMessage?.Invoke(this, "Calculating source hash...");
        if (options.EnableProgress)
        {
            internalHandler = (s, p) =>
                HashProgressChanged?.Invoke(this, new HashProgressEventArgs
                {
                    Progress = p,
                    FileType = FileType.Source
                });
            FileHasher.ProgressChanged += internalHandler;
        }

        var srcHash = await FileHasher.ComputeHashAsync(sourcePath, options.HashAlgorithm, cancellationToken);

        if (internalHandler != null)
            FileHasher.ProgressChanged -= internalHandler;

        StatusMessage?.Invoke(this, "Copying file...");
        await CopyFileAsync(sourcePath, destinationPath, options, cancellationToken);

        StatusMessage?.Invoke(this, "Calculating destination hash...");
        if (options.EnableProgress)
        {
            internalHandler = (s, p) =>
                HashProgressChanged?.Invoke(this, new HashProgressEventArgs
                {
                    Progress = p,
                    FileType = FileType.Destination
                });
            FileHasher.ProgressChanged += internalHandler;
        }

        var dstHash = await FileHasher.ComputeHashAsync(destinationPath, options.HashAlgorithm, cancellationToken);

        if (internalHandler != null)
            FileHasher.ProgressChanged -= internalHandler;

        bool success = string.Equals(srcHash, dstHash, StringComparison.OrdinalIgnoreCase);
        StatusMessage?.Invoke(this, success ? "✅ File copied and verified." : "❌ Checksum mismatch.");

        if (success)
        {
            var srcInfo = new FileInfo(sourcePath);
            File.SetCreationTime(destinationPath, srcInfo.CreationTime);
            File.SetLastWriteTime(destinationPath, srcInfo.LastWriteTime);
            File.SetLastAccessTime(destinationPath, srcInfo.LastAccessTime);
        }

        return success;
    }

    public async Task CopyDirectoryContentAsync(string sourceDir, string destinationDir, FileCopyOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new FileCopyOptions();

        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");

        Directory.CreateDirectory(destinationDir);

        var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string relativePath = Path.GetRelativePath(sourceDir, file);
            string destinationFile = Path.Combine(destinationDir, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);

            var result = await CopyWithVerificationAsync(file, destinationFile, options, cancellationToken);

            if (!result)
                throw new IOException($"Failed to copy file: {file} to {destinationFile}");
        }
    }
    public async Task CopyDirectoryAsync(string sourceDir, string destinationDir, FileCopyOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new FileCopyOptions();

        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");

        string rootFolderName = Path.GetFileName(sourceDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        string newDestinationRoot = Path.Combine(destinationDir, rootFolderName);

        await CopyDirectoryContentAsync(sourceDir, newDestinationRoot, options, cancellationToken);
    }
    private async Task CopyFileAsync(string source, string destination, FileCopyOptions options, CancellationToken cancellationToken)
    {
        const int bufferSize = 81920;
        var buffer = new byte[bufferSize];

        using var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read);
        using var destStream = new FileStream(destination, FileMode.Create, FileAccess.Write);

        long total = sourceStream.Length;
        long copied = 0;
        int read;

        while ((read = await sourceStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await destStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            copied += read;

            if (options.EnableProgress)
            {
                double percent = (double)copied / total * 100;
                CopyProgressChanged?.Invoke(this, percent);
            }
        }
    }
}