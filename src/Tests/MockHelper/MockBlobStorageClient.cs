

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Clients.BlobStorage;
using Clients.BlobStorage.Models;

namespace Tests.MockHelper;

public class MockBlobStorageClient : IBlobStorageClient
{
  public readonly Dictionary<string, string> Files = new();

  public Task<ObjectMetadata> UploadFileAsync(Stream fileStream, string fileName, string destinationPath, CancellationToken cancellationToken = default)
  {
    var tempFilePath = Path.GetTempFileName();
    using var tempFileStream = File.OpenWrite(tempFilePath);
    fileStream.CopyTo(tempFileStream);
    Files.Add(destinationPath, tempFilePath);
    return Task.FromResult(new ObjectMetadata());
  }

  public Task<List<ObjectMetadata>> ListFilesAndFoldersAsync(string path, CancellationToken cancellationToken = default)
  {
    throw new System.NotImplementedException();
  }

  public Task<Stream> GetFileAsync(string path, CancellationToken cancellationToken = default)
  {
    var tempFilePath = Files[path];
    return Task.FromResult<Stream>(File.OpenRead(tempFilePath));
  }

  public string GetLinkFromPath(string path)
  {
    return path;
  }
}



