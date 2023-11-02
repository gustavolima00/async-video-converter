namespace Services.Clients;

public interface IVideoManagerClient
{
}

public class VideoManagerClient : IVideoManagerClient
{
    private readonly VideoManagerClientConfiguration _configuration;

    public VideoManagerClient(VideoManagerClientConfiguration configuration)
    {
        _configuration = configuration;
    }
}