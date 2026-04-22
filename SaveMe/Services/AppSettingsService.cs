using System.Text.Json;
using SaveMe.Models;

namespace SaveMe.Services;

public class AppSettingsService
{
    private const string SettingsFileName = "appsettings.json";
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SaveMe",
        SettingsFileName
    );

    public AppSettings GetSettings()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                string json = File.ReadAllText(SettingsPath);
                JsonContext context = new();
                AppSettings? result = JsonSerializer.Deserialize<AppSettings>(json, context.AppSettings);
                return result ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not read settings file: {ex.Message}");
        }

        return new AppSettings();
    }

    public void SaveSettings(AppSettings settings)
    {
        try
        {
            string? directory = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            JsonSerializerOptions options = new() { WriteIndented = true };
            JsonContext context = new(options);
            string json = JsonSerializer.Serialize(settings, context.AppSettings);
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings: {ex.Message}");
            throw;
        }
    }

    public string GetSaveMePath()
    {
        AppSettings settings = GetSettings();
        if (settings.SaveMePaths == null || settings.SaveMePaths.Count == 0)
        {
            throw new InvalidOperationException(
                "SaveMe path not configured. Please run 'SaveMe --init <path>' to initialize."
            );
        }

        return settings.SaveMePaths[0];
    }

    public List<string> GetSaveMePaths()
    {
        AppSettings settings = GetSettings();
        return settings.SaveMePaths ?? new List<string>();
    }

    public void SetSaveMePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be empty.", nameof(path));
        }

        string absolutePath = Path.GetFullPath(path);
        
        AppSettings settings = GetSettings();
        if (!settings.SaveMePaths.Contains(absolutePath))
        {
            settings.SaveMePaths.Add(absolutePath);
        }
        SaveSettings(settings);
    }

    public void RemoveSaveMePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be empty.", nameof(path));
        }

        string absolutePath = Path.GetFullPath(path);
        AppSettings settings = GetSettings();
        settings.SaveMePaths.Remove(absolutePath);
        SaveSettings(settings);
    }
}
