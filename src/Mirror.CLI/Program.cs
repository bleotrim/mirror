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
            HashAlgorithm = HashAlgorithmType.SHA256
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
                @"/Users/leotrim/Downloads/old/NTFS_Paragon_Driver.dmg",
                @"/Users/leotrim/Desktop/test_file_to_delete",
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
