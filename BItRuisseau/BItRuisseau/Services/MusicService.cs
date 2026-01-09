using BitRuisseau.Models.Protocol;

namespace BitRuisseau.Services;

/// <summary>
/// Service responsible for retrieving music files from the file system.
/// </summary>
public class MusicService
{
    private static readonly string[] SUPPORTED_EXTENSIONS =
    {
        ".mp3", ".wav", ".flac", ".m4a"
    };

    /// <summary>
    /// Retrieves all supported audio files from the given folder.
    /// </summary>
    /// <param name="folderPath">Root folder containing music files</param>
    /// <returns>List of media descriptions</returns>
    public List<MediaDescription> GetSongs(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            return new List<MediaDescription>();
        }

        return Directory
            .GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
            .Where(file =>
                SUPPORTED_EXTENSIONS.Contains(
                    Path.GetExtension(file).ToLower()))
            .Select(CreateMediaDescription)
            .ToList();
    }

    private static MediaDescription CreateMediaDescription(string filePath)
    {
        try
        {
            var tagFile = TagLib.File.Create(filePath);

            return new MediaDescription
            {
                Id = Guid.NewGuid().ToString(),
                Title = string.IsNullOrWhiteSpace(tagFile.Tag.Title)
                    ? Path.GetFileNameWithoutExtension(filePath)
                    : tagFile.Tag.Title,
                Artist = tagFile.Tag.Performers?.FirstOrDefault(),
                Duration = tagFile.Properties.Duration,
                Year = (int)tagFile.Tag.Year,
                Size = tagFile.Length,
                FilePath = filePath
            };
        }
        catch
        {
            // If metadata reading fails, fallback to minimal information
            return new MediaDescription
            {
                Id = Guid.NewGuid().ToString(),
                Title = Path.GetFileNameWithoutExtension(filePath),
                FilePath = filePath
            };
        }
    }
}
