using BitRuisseau.Protocol;

namespace BitRuisseau.Services
{
    public class LocalMediaCenterService
    {
        public MediaCenter Instance { get; }

        public LocalMediaCenterService()
        {
            Instance = new MediaCenter("Eithan");
        }
    }
}
