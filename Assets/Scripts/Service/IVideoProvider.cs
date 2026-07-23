using System.Threading.Tasks;

public interface IVideoProvider
{
    /// <summary>获取视频信息（URL 和时长）</summary>
    Task<VideoInfo> GetVideoAsync(string videoId);
}
