using System.Threading.Tasks;
using UnityEngine;

public class ApiVideoProvider : ApiClient, IVideoProvider
{
    public async Task<VideoInfo> GetVideoAsync(string videoId)
    {
        var resp = await GetAsync<VideoInfo>($"/api/v1/videos/{videoId}");

        if (resp != null && resp.code == 200 && resp.data != null)
        {
            return resp.data;
        }
        Debug.LogWarning($"[ApiVideoProvider] Failed to get video {videoId}: {resp?.message}");
        return null;
    }
}
