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
    Task<List<ObjectMetadata>> ListFilesAndFoldersAsync(string pathPrefix, CancellationToken cancellationToken = default);
    Task<ObjectMetadata> UploadFileAsync(Stream fileStream, string fileName, string destinationPath, CancellationToken cancellationToken = default);
    Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
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

    private string BuildUrl(string path, Dictionary<string, string>? queryParameters = null)
    {
        var builder = new UriBuilder($"{_baseApiUrl}{path}");
        var query = HttpUtility.ParseQueryString(builder.Query);

        if (queryParameters != null)
        {
            foreach (var (key, value) in queryParameters)
            {
                query[key] = value;
            }
        }

        builder.Query = query.ToString();
        return builder.ToString();
    }

    private async Task<HttpContent> GetAsync(string path, Dictionary<string, string>? queryParameters = null, CancellationToken cancellationToken = default)
    {
        var requestUri = BuildUrl(path, queryParameters);
        var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var errorResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new BlobStorageApiException($"Erro na requisição: {errorResponse}");
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new BlobStorageApiException($"Falha ao listar arquivos e pastas. Status code: {response.StatusCode}");
        }

        return response.Content;
    }

    private async Task<Response> GetAndDeserializeAsync<Response>(
        string path,
        Dictionary<string, string>? queryParameters = null,
        CancellationToken cancellationToken = default
    )
    {
        var response = await GetAsync(path, queryParameters, cancellationToken);
        var responseStream = await response.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<Response>(responseStream, cancellationToken: cancellationToken) ?? throw new BlobStorageApiException($"Falha ao desserializar resposta");
    }

    private async Task<Response> PostAndDeserializeAsync<Response>(
        string path,
        HttpContent? content = null,
        Dictionary<string, string>? queryParameters = null,
        CancellationToken cancellationToken = default
    )
    {

        var requestUri = BuildUrl(path, queryParameters);
        var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var errorResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new BlobStorageApiException($"Erro na requisição: {errorResponse}");
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new BlobStorageApiException($"Falha ao listar arquivos e pastas. Status code: {response.StatusCode}");
        }

        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<Response>(responseStream, cancellationToken: cancellationToken) ?? throw new BlobStorageApiException($"Falha ao desserializar resposta");
    }

    public async Task<List<ObjectMetadata>> ListFilesAndFoldersAsync(string pathPrefix, CancellationToken cancellationToken = default)
    {
        return await GetAndDeserializeAsync<List<ObjectMetadata>>("/list", new()
        {
            ["path_prefix"] = pathPrefix
        }
        , cancellationToken);
    }

    public async Task<ObjectMetadata> UploadFileAsync(Stream fileStream, string fileName, string destinationPath, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent
        {
            { new StreamContent(fileStream), "file", fileName }
        };

        return await PostAndDeserializeAsync<ObjectMetadata>("/upload", content, new()
        {
            ["folder_path"] = destinationPath,
            ["file_name"] = fileName
        }, cancellationToken);
    }

    public async Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var response = await GetAsync("/get-file", new()
        {
            ["file_path"] = filePath
        }, cancellationToken);
        return await response.ReadAsStreamAsync(cancellationToken);
    }
}
