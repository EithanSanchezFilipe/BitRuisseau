namespace BitRuisseau.Models.Protocol
{
    /// <summary>
    /// Un objet de ce type décrit une médiathèque,
    /// dans le but de pouvoir l'afficher dans une médiathèque distante
    /// </summary>
    public class MediaCenter
    {
        /// <summary>
        /// identifiant unique permettant de distinguer les médiathèques présente sur le réseau
        /// </summary>
        public string Id { get; init; } = Guid.NewGuid().ToString();
        
        public string? Name { get; set; }
        public MediaCenter(string name) 
        {
            Name = name;
        }
    }
}
