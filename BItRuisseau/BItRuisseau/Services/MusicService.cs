using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BItRuisseau.Models;
using TagLib;

namespace BItRuisseau.Services
{
    public class MusicService
    {
        public List<Music> GetMusics(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                return new List<Music>();

            string[] extensions = new[] { ".mp3", ".wav", ".flac", ".m4a" };

            var musics = Directory
                .GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()))
                .Select(file =>
                {
                    try
                    {
                        var tagFile = TagLib.File.Create(file);

                        return new Music
                        {
                            Name = Path.GetFileName(file),
                            FilePath = file,
                            Title = string.IsNullOrWhiteSpace(tagFile.Tag.Title)
                                        ? Path.GetFileNameWithoutExtension(file)
                                        : tagFile.Tag.Title,
                            Author = tagFile.Tag.Performers ?? Array.Empty<string>(),
                            Duration = tagFile.Properties.Duration
                        };
                    }
                    catch
                    {
                        return new Music
                        {
                            Name = Path.GetFileName(file),
                            FilePath = file,
                            Title = Path.GetFileNameWithoutExtension(file),
                            Author = Array.Empty<string>(),
                            Duration = TimeSpan.Zero
                        };
                    }
                })
                .ToList();

            return musics;
        }
    }
}
