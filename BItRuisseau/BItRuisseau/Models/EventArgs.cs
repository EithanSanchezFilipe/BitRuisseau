using BitRuisseau.Models.Protocol;

namespace BitRuisseau.Models
{
    /// <summary>
    /// Event arguments for when a media list is received from another MediaCenter.
    /// </summary>
    public class MediaListReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the ID of the MediaCenter that sent the media list.
        /// </summary>
        public string SenderId { get; init; }

        /// <summary>
        /// Gets the list of media descriptions received.
        /// </summary>
        public List<MediaDescription> Medias { get; init; }
    }

    /// <summary>
    /// Event arguments for when a catalog request is received from another MediaCenter.
    /// </summary>
    public class CatalogRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the ID of the MediaCenter that requested the catalog.
        /// </summary>
        public string RequesterId { get; init; }
    }
}
