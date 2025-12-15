namespace BitRuisseau.Protocol
{
    public enum MessageType
    {
        WHO_IS_THERE,       // demande à toutes les médiathèques de s'annoncer publiquement sur le réseau
        I_AM_HERE,          // Annonce sa présence sur le réseau
        I_AM_OUT,           // Annonce son départ du réseau
        CATALOG_REQUEST,    // demande son catalogue à une médiathèque précise (i.e. utiliser RecipientId)
        CATALOG,            // retourne son catalogue à la médiathèque qui avait émis la demande (i.e. utiliser RecipientId)
        FRAGMENT_REQUEST,   // demande tout ou partie d'un média à une médiathèque précise  (i.e. utiliser RecipientId)
        FRAGMENT,           // retourne le fragment demandé
    }
}
