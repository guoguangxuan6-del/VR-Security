using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class LocalVideoProvider : IVideoProvider
{
    public Task<VideoInfo> GetVideoAsync(string videoId)
    {
        string localPath = Path.Combine(Application.streamingAssetsPath, "Videos", videoId + ".mp4");
        var info = new VideoInfo
        {
            videoId = videoId,
            url = localPath,
            durationSeconds = 0
        };
        return Task.FromResult(info);
    }
}
