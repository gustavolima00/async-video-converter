using Microsoft.Extensions.Configuration;

namespace Clients.BlobStorage;

public class BlobStorageClientConfiguration
{
    public string BlobStorageUrl { get; set; } = "";

    public static BlobStorageClientConfiguration Build(IConfigurationSection? configuration)
    {
        var config = new BlobStorageClientConfiguration();
        config.BlobStorageUrl = configuration?.GetSection(nameof(config.BlobStorageUrl)).Value ?? "";
        if (string.IsNullOrEmpty(config.BlobStorageUrl))
            throw new Exception("BlobStorageUrl is required");
        return config;
    }
}