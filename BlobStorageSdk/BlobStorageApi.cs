using Microsoft.Extensions.Configuration;
using System.Text.Json;
using BlobStorageSdk.Models;

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

    public BlobStorageApi(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseApiUrl = configuration["FILE_MANAGER_API_BASE_URL"] ?? "";
    }

    public async Task<List<ObjectMetadata>> ListFilesAndFoldersAsync(string pathPrefix)
    {

        var response = await _httpClient.GetAsync($"{_baseApiUrl}/list?path_prefix={Uri.EscapeDataString(pathPrefix)}");

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

        // Envie a requisição POST
        var response = await _httpClient.PostAsync($"{_baseApiUrl}/upload?folder_path={Uri.EscapeDataString(destinationPath)}", content);

        if (!response.IsSuccessStatusCode)
        {
            throw new BlobStorageApiException($"Falha ao fazer upload do arquivo. Status code: {response.StatusCode}");
        }

        var responseStream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<ObjectMetadata>(responseStream) ?? throw new BlobStorageApiException($"Falha ao desserializar resposta. Status code: {response.StatusCode}");
        return result;
    }

    public async Task<Stream> GetFileAsync(string filePath){
        var response = await _httpClient.GetAsync($"{_baseApiUrl}/get-file?file_path={Uri.EscapeDataString(filePath)}");

        if (!response.IsSuccessStatusCode)
        {
            throw new BlobStorageApiException($"Falha ao fazer download do arquivo. Status code: {response.StatusCode}");
        }

        return await response.Content.ReadAsStreamAsync();
    }
}
