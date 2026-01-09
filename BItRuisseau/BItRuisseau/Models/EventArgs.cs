using BitRuisseau.Protocol;

namespace BitRuisseau.Models
{
    public class MediaListReceivedEventArgs : EventArgs
    {
        public string SenderId { get; init; }
        public List<MediaDescription> Medias { get; init; }
    }
    public class CatalogRequestEventArgs : EventArgs
    {
        public string RequesterId { get; init; }
    }
}
