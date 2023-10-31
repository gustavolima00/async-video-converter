using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;
using FileManager;

namespace FileManager.Tests;

public class FileManagerApiServiceTests
{
    [Fact]
    public async Task TestSaveFileAsync()
    {
        // Mock IConfiguration
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.SetupGet(c => c["FILE_MANAGER_API_BASE_URL"]).Returns("http://localhost:5000");
        var httpClient = new HttpClient();

        // Criar o serviço com os mocks
        var fileManagerApi = new FileManagerApi(httpClient, configurationMock.Object);
        var result = await fileManagerApi.ListFilesAndFoldersAsync(".");

        // Assert with an empty list
        Assert.Empty(result);
    }
}

