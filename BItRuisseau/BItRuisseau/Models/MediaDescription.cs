using System.Text.Json.Serialization;

namespace BitRuisseau.Protocol
{
    /// <summary>
    /// Un objet de ce type décrit un média audio,
    /// dans le but de pouvoir l'afficher dans une médiathèque distante
    /// </summary>
    public class MediaDescription
    {
        /// <summary>
        /// identifiant unique non persistant.
        /// il n'est utilisé que durant le transfert entre deux noeuds
        /// </summary>
        public string Id { get; init; } = Guid.NewGuid().ToString();
        
        public string? Title { get; set; }

        public string? Artist { get; set; }

        /// <summary>
        /// 0 si l'année n'est pas connue
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// En bytes
        /// </summary>
        public long Size { get; set; }

        public TimeSpan Duration { get; set; }
        
        /// <summary>
        /// optionnel : genre musical (techno, hip-hop, rock, ...)
        /// </summary>
        public string? Category { get; set; }
        
        /// <summary>
        /// optionnel
        /// </summary>
        public string? Featuring { get; set; }

        [JsonIgnore]
        public string? FilePath { get; set; }
    }
}
