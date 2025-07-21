using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var copier = new FileCopier();
        var cts = new CancellationTokenSource();

        copier.StatusMessage += (s, msg) => Console.WriteLine($"[STATUS] {msg}");
        copier.CopyProgressChanged += (s, p) => Console.WriteLine($"[COPY] {p:F2}%");
        copier.HashProgressChanged += (s, e) =>
            Console.WriteLine($"[HASH - {e.FileType}] {e.Progress:F2}%");

        var options = new FileCopyOptions
        {
            Overwrite = true,
            EnableProgress = true,
            HashAlgorithm = HashAlgorithmType.SHA256
        };

        _ = Task.Run(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine("Copy cancelled by user");
                        cts.Cancel();
                        break;
                    }
                }
                Thread.Sleep(100);
            }
        });

        try
        {
            await copier.CopyDirectoryContentAsync(
                @"/Users/leotrim/Downloads",
                @"/Users/leotrim/Desktop/fortest",
                options,
                cts.Token
            );

            Console.WriteLine("Directory copied successfully");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Copy failed");
        }
    }
}
