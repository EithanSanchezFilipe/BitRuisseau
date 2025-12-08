namespace BItRuisseau.Models
{
    public class Music
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string[] Author { get; set; } = Array.Empty<string>();
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        public string? FilePath { get; set; }
        public uint Year { get; set; } = 0;
    }
}
