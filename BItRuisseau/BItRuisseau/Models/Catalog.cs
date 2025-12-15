namespace BitRuisseau.Protocol
{
    /// <summary>
    /// Un objet de ce type décrit le catalogue d'une médiathèque
    /// </summary>
    public class Catalog
    {
        /// <summary>
        /// la médiathèque dont ceci est le catalogue
        /// </summary>
        public string? MediaCenterId { get; set; }
        
        public List<MediaDescription>? Medias { get; set; }
        
    }
}
