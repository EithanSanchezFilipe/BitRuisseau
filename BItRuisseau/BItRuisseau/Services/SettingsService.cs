using System.Text.Json;
using CommunityToolkit.Maui.Storage;
using BitRuisseau.Models;

namespace BitRuisseau.Services;

public class SettingsService
{
    private readonly IFolderPicker _folderPicker;

    public Settings Current { get; private set; } = new();

    private readonly string SETTINGS_FILE =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BitRuisseau", "AppSettings.json");

    public SettingsService(IFolderPicker folderPicker)
    {
        _folderPicker = folderPicker;
        Load();
    }

    public void Load()
    {
        try
        {
            if (File.Exists(SETTINGS_FILE))
            {
                string json = File.ReadAllText(SETTINGS_FILE);
                Current = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
        }
        catch
        {
            Current = new Settings();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SETTINGS_FILE)!);
        File.WriteAllText(SETTINGS_FILE, JsonSerializer.Serialize(Current));
    }

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
}
