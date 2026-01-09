namespace BitRuisseau.Models.Protocol
{
    /// <summary>
    /// Un objet de ce type contient une partie d'un média
    /// </summary>
    public class Fragment
    {
        /// <summary>
        /// le média auquel ce fragment appartient
        /// </summary>
        public string MediaId { get; set; }
        
        /// <summary>
        /// la position dans le fichier où ce fragment doit commencer
        /// si la valeur est -1, cela signifie que c'est le début du fichier
        /// </summary>
        public int StartIndex { get; set; }
        
        /// <summary>
        /// la position dans le fichier ou se trouve le dernier byte de ce fragment
        /// si la valeur est -1, cela signifie que c'est la fin du fichier
        /// </summary>
        public int EndIndex { get; set; }
        
        /// <summary>
        /// le contenu.
        /// il s'agit de binaire encodé en base64
        /// https://fr.wikipedia.org/wiki/Base64
        /// https://learn.microsoft.com/fr-fr/dotnet/api/system.convert.tobase64string?view=net-8.0
        ///
        /// il est vide quand l'objet est utilisé pour faire une demande de fragment (MessageType.FRAGMENT_REQUEST)
        /// 
        /// </summary>
        public string? Content { get; set; }

    }
}
