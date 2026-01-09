using BitRuisseau.Models.Protocol;

namespace BitRuisseau.Services;

/// <summary>
/// Provides access to the local media center instance.
/// </summary>
public class LocalMediaCenterService
{
    private const string DEFAULT_MEDIA_CENTER_NAME = "Eithan";

    /// <summary>
    /// Local media center instance.
    /// </summary>
    public MediaCenter Instance { get; }

    /// <summary>
    /// Initializes a new instance of the LocalMediaCenterService.
    /// </summary>
    public LocalMediaCenterService()
    {
        Instance = new MediaCenter(DEFAULT_MEDIA_CENTER_NAME);
    }
}
