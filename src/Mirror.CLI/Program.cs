using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var copier = new FileCopier();

        copier.CopyProgressChanged += (s, percent) =>
        {
            Console.WriteLine($"Progresso copia: {percent:F2}%");
        };

        copier.HashProgressChanged += (s, percent) =>
        {
            Console.WriteLine($"Progresso hash: {percent:F2}%");
        };

        copier.StatusMessage += (s, msg) =>
        {
            Console.WriteLine($"Stato: {msg}");
        };

        var options = new FileCopyOptions
        {
            Overwrite = true,
            EnableProgress = true,
            HashAlgorithm = "SHA256"
        };

        using var cts = new CancellationTokenSource();

        _ = Task.Run(() =>
        {
            Console.WriteLine("Premi ESC per annullare...");
            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    cts.Cancel();
                    break;
                }
                Thread.Sleep(100); // Riduce il carico della CPU
            }
        });

        try
        {
            bool success = await copier.CopyWithVerificationAsync(
                @"C:\Users\leba\Videos\Screen Recordings\Screen Recording 2024-06-26 154907.mp4",
                @"C:\Users\leba\Desktop\test_large_file",
                options,
                cts.Token);

            Console.WriteLine(success
                ? "✅ Copia riuscita!"
                : "❌ Errore nella copia o checksum non corrispondente.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("⚠️ Operazione annullata.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Errore: {ex.Message}");
        }
    }
}
