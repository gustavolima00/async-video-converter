using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;
using BlobStorageSdk;
using System.IO;

namespace BlobStorageSdk.Tests;

public class BlobStorageApiTests
{
    public static IBlobStorageApi BuildBlobStorageApiInstance(HttpResponseMessage mockApiResponse)
    {
        var httpClient = new HttpClient();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.SetupGet(c => c["FILE_MANAGER_API_BASE_URL"]).Returns("http://mock-blob-storage");

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(mockApiResponse);
        httpClient = new HttpClient(mockHttpMessageHandler.Object);
        return new BlobStorageApi(httpClient, configurationMock.Object);
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
                    ""name"": ""uploaded_file.pdf"",
                    ""path"": ""folder/uploaded_file.pdf"",
                    ""size"": 4096,
                    ""type"": ""file""
                }",
                Encoding.UTF8,
                "application/json"
            )
        };
        var blobStorageApi = BuildBlobStorageApiInstance(mockResponse);

        var fileContent = "File content is this - The quick brown fox jumps over the lazy dog";
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        var result = await blobStorageApi.UploadFileAsync(fileStream, "uploaded_file.pdf", "folder");
        Assert.Equal("uploaded_file.pdf", result.Name);
        Assert.Equal("file", result.Type);
        Assert.Equal(4096, result.Size);
        Assert.Equal(new DateTime(2023, 10, 31, 15, 42, 28), result.LastModified);
        Assert.Equal(new DateTime(2023, 10, 31, 15, 42, 28), result.CreatedAt);
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
        var result = await blobStorageApi.GetFileAsync("folder/uploaded_file.pdf");
        Assert.Equal(fileContent, new StreamReader(result).ReadToEnd());
    }
}
