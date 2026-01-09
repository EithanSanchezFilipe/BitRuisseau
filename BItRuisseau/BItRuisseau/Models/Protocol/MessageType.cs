namespace BitRuisseau.Models.Protocol
{
    public enum MessageType
    {
        HELLO,                      //annonce de départ
        GOOD_BYE,                   //annonce d’absence
        TIME_SYNC,                  //envoie une référence de temps
        TOWN_ENVIRONMENT,           //Infos sur l’environnement actuel
        HOUSE_STATUS,               //Données de monitoring en réponse
        HOUSE_STATUS_REQUEST,       //Demande de status de la part de PowerWatch
        CASH,                       //Porteur d'une transaction de cash (CashTransaction)
        POWER,                      //Porteur d'une transaction d'énergie (PowerTransaction)
        QUIT,

        // Bitruisseau
        // Les messages de ces types circulent sur le topic `powercher/bitruisseau`

        WHO_IS_THERE,               // demande à toutes les médiathèques de s'annoncer publiquement sur le réseau
        I_AM_HERE,                  // Annonce sa présence sur le réseau
        I_AM_OUT,                   // Annonce son départ du réseau
        CATALOG_REQUEST,            // demande son catalogue à une médiathèque précise (i.e. utiliser RecipientId)
        CATALOG,                    // retourne son catalogue à la médiathèque qui avait émis la demande (i.e. utiliser RecipientId)
        FRAGMENT_REQUEST,           // demande tout ou partie d'un média à une médiathèque précise  (i.e. utiliser RecipientId)
        FRAGMENT
    }
}
