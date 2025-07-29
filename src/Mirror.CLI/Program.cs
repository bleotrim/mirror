using CommandLine;
using System;
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
}
