using System.Text.Json;
using CommunityToolkit.Maui.Storage;
using BitRuisseau.Models;

namespace BitRuisseau.Services;

/// <summary>
/// Service responsible for loading, saving and updating application settings.
/// </summary>
public class SettingsService
{
    private readonly IFolderPicker _folderPicker;
    public Settings Current { get; private set; } = new();

    private const string SETTINGS_FILE =
        "BitRuisseau/AppSettings.json";

    /// <summary>
    /// Initializes a new instance of the SettingsService.
    /// </summary>
    /// <param name="folderPicker">Folder picker service</param>
    public SettingsService(IFolderPicker folderPicker)
    {
        _folderPicker = folderPicker;
        Load();
    }

    /// <summary>
    /// Loads settings from the local configuration file.
    /// </summary>
    public void Load()
    {
        try
        {
            var filePath = GetSettingsFilePath();

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                Current = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
        }
        catch
        {
            // If loading fails, default settings are used
            Current = new Settings();
        }
    }

    /// <summary>
    /// Saves current settings to the local configuration file.
    /// </summary>
    public void Save()
    {
        var filePath = GetSettingsFilePath();

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, JsonSerializer.Serialize(Current));
    }

    /// <summary>
    /// Opens a folder picker to select the music directory.
    /// </summary>
    /// <returns>True if a folder was selected successfully</returns>
    public async Task<bool> PickMusicFolderAsync()
    {
        try
        {
            var result = await _folderPicker.PickAsync();

            if (!result.IsSuccessful)
                return false;

            Current.MusicFolderPath = result.Folder.Path;
            Save();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GetSettingsFilePath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            SETTINGS_FILE);
    }
}
