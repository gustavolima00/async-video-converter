using Microsoft.Extensions.Configuration;
using System.Text.Json;
using BlobStorageSdk.Models;
using System.Web;

namespace BlobStorageSdk;

public class BlobStorageApiException : Exception
{
    public BlobStorageApiException(string message) : base(message)
    {
    }
}

public interface IBlobStorageApi
{
    Task<List<ObjectMetadata>> ListFilesAndFoldersAsync(string pathPrefix);
    Task<ObjectMetadata> UploadFileAsync(Stream fileStream, string fileName, string destinationPath);
    Task<Stream> GetFileAsync(string filePath);
}

public class BlobStorageApi : IBlobStorageApi
{
    private readonly HttpClient _httpClient;
    private readonly string _baseApiUrl;

    public BlobStorageApi(HttpClient httpClient, BlobStorageSdkConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseApiUrl = configuration.BlobStorageUrl;
    }

    private string BuildUrl(string path, Dictionary<string, string> queryParameters)
    {
        var builder = new UriBuilder($"{_baseApiUrl}{path}");
        var query = HttpUtility.ParseQueryString(builder.Query);

        foreach (var (key, value) in queryParameters)
        {
            query[key] = value;
        }

        builder.Query = query.ToString();
        return builder.ToString();
    }

    public async Task<List<ObjectMetadata>> ListFilesAndFoldersAsync(string pathPrefix)
    {
        var requestUri = BuildUrl("/list", new()
        {
            ["path_prefix"] = pathPrefix
        });

        var response = await _httpClient.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
        {
            throw new BlobStorageApiException($"Falha ao listar arquivos e pastas. Status code: {response.StatusCode}");
        }

        var responseStream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<List<ObjectMetadata>>(responseStream) ?? throw new BlobStorageApiException($"Falha ao desserializar resposta. Status code: {response.StatusCode}");
        return result;
    }

    public async Task<ObjectMetadata> UploadFileAsync(Stream fileStream, string fileName, string destinationPath)
    {
        using var content = new MultipartFormDataContent
        {
            { new StreamContent(fileStream), "file", fileName }
        };
        var requestUri = BuildUrl("/upload", new()
        {
            ["folder_path"] = destinationPath,
            ["file_name"] = fileName
        });

        var response = await _httpClient.PostAsync(requestUri, content);

        if (!response.IsSuccessStatusCode)
        {
            throw new BlobStorageApiException($"Falha ao fazer upload do arquivo. Status code: {response.StatusCode}");
        }

        var responseStream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<ObjectMetadata>(responseStream) ?? throw new BlobStorageApiException($"Falha ao desserializar resposta. Status code: {response.StatusCode}");
        return result;
    }

    public async Task<Stream> GetFileAsync(string filePath)
    {
        var requestUri = BuildUrl("/get-file", new()
        {
            ["file_path"] = filePath
        });

        var response = await _httpClient.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
        {
            throw new BlobStorageApiException($"Falha ao fazer download do arquivo. Status code: {response.StatusCode}");
        }

        return await response.Content.ReadAsStreamAsync();
    }
}
