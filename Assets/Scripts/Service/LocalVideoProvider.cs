using System.IO;
using UnityEngine;

public class LocalVideoProvider : IVideoProvider
{
    public string GetVideoPath(string videoId)
    {
        // 假设视频文件以 .mp4 结尾
        return Path.Combine(Application.streamingAssetsPath, "Videos", videoId + ".mp4");
    }

    public bool HasVideo(string videoId)
    {
        string path = GetVideoPath(videoId);
        // StreamingAssets 在大多数平台可用 File.Exists 检查
        return File.Exists(path);
    }
}