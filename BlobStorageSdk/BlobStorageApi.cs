using System;
using System.Net.Http;
using System.Threading.Tasks;
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
}

public class BlobStorageApi : IBlobStorageApi
{
    private readonly HttpClient _httpClient;
    private readonly string _baseApiUrl;

    public BlobStorageApi(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseApiUrl = configuration["FILE_MANAGER_API_BASE_URL"];
    }

    public async Task<List<ObjectMetadata>> ListFilesAndFoldersAsync(string pathPrefix)
    {

        var response = await _httpClient.GetAsync($"{_baseApiUrl}/list?path_prefix={pathPrefix}");

        if (!response.IsSuccessStatusCode)
        {
            throw new BlobStorageApiException($"Falha ao listar arquivos e pastas. Status code: {response.StatusCode}");
        }

        var responseStream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<List<ObjectMetadata>>(responseStream);
        if (result is null)
        {
            throw new BlobStorageApiException($"Falha ao desserializar resposta. Status code: {response.StatusCode}");
        }
        return result;
    }
}
