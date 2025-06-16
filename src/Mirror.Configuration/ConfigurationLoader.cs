using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace MyApp.Configuration;

public static class ConfigurationLoader
{
    private const string DefaultFileName = "appsettings.json";

    public static IConfiguration LoadConfiguration(string fileName = DefaultFileName)
    {
        EnsureFileExists(fileName);
        
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(fileName, optional: false, reloadOnChange: true)
            .Build();

        return config;
    }

    private static void EnsureFileExists(string fileName)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, fileName);

        if (!File.Exists(fullPath))
        {
            var defaultSettings = new
            {
                AppSettings = new
                {
                    ApplicationName = "My Default App",
                    DefaultLanguage = "it"
                }
            };

            var json = JsonSerializer.Serialize(defaultSettings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(fullPath, json);
        }
    }
}
