

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Clients.FFmpeg;
using Repositories.Models;
using Xabe.FFmpeg;

namespace Tests.MockHelper;

class MockMediaInfo : MediaMetadata, IMediaInfo
{
  IEnumerable<IStream> IMediaInfo.Streams => throw new System.NotImplementedException();

  IEnumerable<IVideoStream> IMediaInfo.VideoStreams => throw new System.NotImplementedException();

  IEnumerable<IAudioStream> IMediaInfo.AudioStreams => throw new System.NotImplementedException();

  IEnumerable<ISubtitleStream> IMediaInfo.SubtitleStreams => throw new System.NotImplementedException();
}

public class MockFFmpegClient : IFFmpegClient
{
  public Task<Stream> ConvertToMp4(Stream stream, string fileExtension, CancellationToken cancellationToken = default)
  {
    return Task.FromResult(stream);
  }

  public Task<IMediaInfo> GetFileMetadata(Stream stream, string fileExtension, CancellationToken cancellationToken = default)
  {
    return Task.FromResult<IMediaInfo>(new MockMediaInfo());
  }
}



