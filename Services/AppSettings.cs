using System.Text.Json;
using System.Text.Json.Serialization;
using vMixSessionCleaner.Localization;

namespace vMixSessionCleaner.Services;

public sealed class AppSettings
{
    private const string DefaultProjectPath = @"C:\";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string ProjectPath { get; set; } = DefaultProjectPath;
    public AppLanguage? Language { get; set; }

    private static string SettingsDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "vMixSessionCleaner");

    private static string SettingsFilePath => Path.Combine(SettingsDirectory, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsFilePath))
                return new AppSettings();

            var json = File.ReadAllText(SettingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            if (string.IsNullOrWhiteSpace(settings.ProjectPath))
                settings.ProjectPath = DefaultProjectPath;
            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(SettingsDirectory);
        var json = JsonSerializer.Serialize(this, JsonOptions);
        File.WriteAllText(SettingsFilePath, json);
    }

    public void SaveProjectPath(string path)
    {
        ProjectPath = string.IsNullOrWhiteSpace(path) ? DefaultProjectPath : path.Trim();
        Save();
    }

    public void SaveLanguage(AppLanguage language)
    {
        Language = language;
        Save();
    }
}
