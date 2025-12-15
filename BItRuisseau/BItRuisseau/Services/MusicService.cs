using BitRuisseau.Protocol;

namespace BitRuisseau.Services
{
    public class MusicService
    {
        public List<MediaDescription> MyMusicList { get; set; } = new();
        public List<MediaDescription> CurrentMusicList { get; set; } = new();

        public event Action? OnMusicListChanged;

        public void SetCurrentList(List<MediaDescription> list)
        {
            CurrentMusicList = list;
            OnMusicListChanged?.Invoke();
        }

        public void GetMusics(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                MyMusicList = new();
                return;
            }

            string[] extensions = { ".mp3", ".wav", ".flac", ".m4a" };

            MyMusicList = Directory
                .GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()))
                .Select(file =>
                {
                    try
                    {
                        var tagFile = TagLib.File.Create(file);

                        return new MediaDescription
                        {
                            Id = Guid.NewGuid().ToString(),
                            Title = string.IsNullOrWhiteSpace(tagFile.Tag.Title)
                                ? Path.GetFileNameWithoutExtension(file)
                                : tagFile.Tag.Title,
                            Artist = tagFile.Tag.Performers?.FirstOrDefault(),
                            Duration = tagFile.Properties.Duration,
                            Year = (int)tagFile.Tag.Year,
                            Size = tagFile.Length,
                            FilePath = file
                        };
                    }
                    catch
                    {
                        return new MediaDescription
                        {
                            Id = Guid.NewGuid().ToString(),
                            Title = Path.GetFileNameWithoutExtension(file),
                            FilePath = file
                        };
                    }
                })
                .ToList();
            CurrentMusicList = MyMusicList;
        }
    }
}
