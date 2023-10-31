using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using FileManager.Models;

namespace FileManager;

public class FileManagerRequestException : Exception
{
    public FileManagerRequestException(string message) : base(message)
    {
    }
}

public interface IFileManagerApi
{
    Task<List<ObjectMetadata>> ListFilesAndFoldersAsync(string pathPrefix);
}

public class FileManagerApi : IFileManagerApi
{
    private readonly HttpClient _httpClient;
    private readonly string _baseApiUrl;

    public FileManagerApi(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseApiUrl = configuration["FILE_MANAGER_API_BASE_URL"];
    }

    public async Task<List<ObjectMetadata>> ListFilesAndFoldersAsync(string pathPrefix)
    {

        var response = await _httpClient.GetAsync($"{_baseApiUrl}/list?path_prefix={pathPrefix}");

        if (!response.IsSuccessStatusCode)
        {
            throw new FileManagerRequestException($"Falha ao listar arquivos e pastas. Status code: {response.StatusCode}");
        }

        var responseStream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<List<ObjectMetadata>>(responseStream);
        if (result is null)
        {
            throw new FileManagerRequestException($"Falha ao desserializar resposta. Status code: {response.StatusCode}");
        }
        return result;
    }
}
