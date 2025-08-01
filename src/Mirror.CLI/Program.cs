using CommandLine;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task<int> Main(string[] args)
    {
        return await Parser.Default.ParseArguments<CopyFileOptions, CopyDirContentOptions, CopyDirOptions>(args)
            .MapResult(
                (CopyFileOptions opts) => RunCopyFile(opts),
                (CopyDirContentOptions opts) => RunCopyDirContent(opts),
                (CopyDirOptions opts) => RunCopyDir(opts),
                errs => Task.FromResult(1)
            );
    }

    private static async Task<int> RunCopyFile(CopyFileOptions opts)
    {
        var copier = CreateCopier(opts.EnableProgress);
        var options = new FileCopyOptions
        {
            Overwrite = opts.Overwrite,
            EnableProgress = opts.EnableProgress
        };

        if (!File.Exists(opts.SourcePath))
        {
            Console.Error.WriteLine("Source file does not exist.");
            return 1;
        }

        long sizeInBytes = new FileInfo(opts.SourcePath).Length;

        Console.WriteLine("Copying File:");
        Console.WriteLine($"From: {opts.SourcePath}");
        Console.WriteLine($"To:   {opts.DestinationPath}");
        Console.WriteLine($"Size: {FormatSize(sizeInBytes)}");
        Console.WriteLine();

        bool result = await copier.CopyWithVerificationAsync(opts.SourcePath, opts.DestinationPath, options);
        return result ? 0 : 1;
    }

    private static async Task<int> RunCopyDirContent(CopyDirContentOptions opts)
    {
        var copier = CreateCopier(opts.EnableProgress);
        var options = new FileCopyOptions
        {
            Overwrite = opts.Overwrite,
            EnableProgress = opts.EnableProgress
        };

        if (!Directory.Exists(opts.SourceDir))
        {
            Console.Error.WriteLine("Source directory does not exist.");
            return 1;
        }

        var files = Directory.GetFiles(opts.SourceDir, "*", SearchOption.AllDirectories);
        long totalBytes = files.Sum(f => new FileInfo(f).Length);

        Console.WriteLine("Copying Directory Content:");
        Console.WriteLine($"From:  {opts.SourceDir}");
        Console.WriteLine($"To:    {opts.DestinationDir}");
        Console.WriteLine($"Files: {files.Length}");
        Console.WriteLine($"Total Size: {FormatSize(totalBytes)}");
        Console.WriteLine();

        await copier.CopyDirectoryContentAsync(opts.SourceDir, opts.DestinationDir, options);
        return 0;
    }

    private static async Task<int> RunCopyDir(CopyDirOptions opts)
    {
        var copier = CreateCopier(opts.EnableProgress);
        var options = new FileCopyOptions
        {
            Overwrite = opts.Overwrite,
            EnableProgress = opts.EnableProgress
        };

        if (!Directory.Exists(opts.SourceDir))
        {
            Console.Error.WriteLine("Source directory does not exist.");
            return 1;
        }

        var files = Directory.GetFiles(opts.SourceDir, "*", SearchOption.AllDirectories);
        long totalBytes = files.Sum(f => new FileInfo(f).Length);
        string rootFolderName = Path.GetFileName(opts.SourceDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        string newDestinationRoot = Path.Combine(opts.DestinationDir, rootFolderName);

        Console.WriteLine("Copying Directory:");
        Console.WriteLine($"From:  {opts.SourceDir}");
        Console.WriteLine($"To:    {newDestinationRoot}");
        Console.WriteLine($"Files: {files.Length}");
        Console.WriteLine($"Total Size: {FormatSize(totalBytes)}");
        Console.WriteLine();

        await copier.CopyDirectoryAsync(opts.SourceDir, opts.DestinationDir, options);
        return 0;
    }

    private static FileCopier CreateCopier(bool showProgress)
    {
        var copier = new FileCopier();

        copier.StatusMessage += (s, msg) => Console.WriteLine(msg);

        if (showProgress)
        {
            copier.CopyProgressChanged += (s, p) =>
            {
                Console.Write($"\rCopy Progress: {p:F2}%   ");
            };

            copier.HashProgressChanged += (s, e) =>
            {
                Console.Write($"\rHashing ({e.FileType}): {e.Progress:F2}%   ");
            };
        }

        return copier;
    }

    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
