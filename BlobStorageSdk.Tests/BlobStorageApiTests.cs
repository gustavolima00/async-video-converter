using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using System.IO;
using BlobStorageSdk.Models;

namespace BlobStorageSdk.Tests;

public class BlobStorageApiTests
{
    public static IBlobStorageApi BuildBlobStorageApiInstance(HttpResponseMessage mockApiResponse)
    {
        var httpClient = new HttpClient();
        var configuration = new BlobStorageSdkConfiguration
        {
            BlobStorageUrl = "https://blob-storage-api.com"
        };

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(mockApiResponse);
        httpClient = new HttpClient(mockHttpMessageHandler.Object);
        return new BlobStorageApi(httpClient, configuration);
    }

    [Fact]
    public async Task TestSaveFileAsync()
    {
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                @"[
                    {
                        ""created_at"": ""2023-10-31T15:42:28"",
                        ""last_modified"": ""2023-10-31T15:42:28"",
                        ""name"": ""abc"",
                        ""path"": ""abc"",
                        ""size"": 4096,
                        ""type"": ""folder""
                    }
                ]",
                Encoding.UTF8,
                "application/json"
            )
        };
        var blobStorageApi = BuildBlobStorageApiInstance(mockResponse);
        var result = await blobStorageApi.ListFilesAndFoldersAsync(".");
        Assert.Single(result);
        Assert.Equal("abc", result[0].Name);
        Assert.Equal("folder", result[0].Type);
        Assert.Equal(4096, result[0].Size);
        Assert.Equal(new DateTime(2023, 10, 31, 15, 42, 28), result[0].LastModified);
        Assert.Equal(new DateTime(2023, 10, 31, 15, 42, 28), result[0].CreatedAt);
    }

    [Fact]
    public async Task TestUploadFileAsync()
    {
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                @"{
                    ""created_at"": ""2023-10-31T15:42:28"",
                    ""last_modified"": ""2023-10-31T15:42:28"",
                    ""name"": ""uploaded_file.txt"",
                    ""path"": ""folder/uploaded_file.txt"",
                    ""size"": 66,
                    ""type"": ""file""
                }",
                Encoding.UTF8,
                "application/json"
            )
        };
        var blobStorageApi = BuildBlobStorageApiInstance(mockResponse);

        var fileContent = "File content is this - The quick brown fox jumps over the lazy dog";
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        var result = await blobStorageApi.UploadFileAsync(fileStream, "uploaded_file.txt", "folder");
        Assert.Equal("uploaded_file.txt", result.Name);
        Assert.Equal("file", result.Type);
        Assert.Equal("folder/uploaded_file.txt", result.Path);
    }

    [Fact]
    public async Task TestGetFileAsync()
    {
        var fileContent = "File content is this - The quick brown fox jumps over the lazy dog";
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        // Mock da resposta HTTP
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(fileStream)
        };

        var blobStorageApi = BuildBlobStorageApiInstance(mockResponse);
        var result = await blobStorageApi.GetFileAsync("folder/uploaded_file.txt");
        Assert.Equal(fileContent, new StreamReader(result).ReadToEnd());
    }
}
