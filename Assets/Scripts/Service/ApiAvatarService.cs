using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEngine;

public class ApiAvatarService : IAvatarService
{
    private const string BaseUrl = "http://123.57.30.132:8080";

    private static readonly HttpClient http = new HttpClient();

    /// <summary>
    /// 上传头像文件到后端
    /// </summary>
    public async Task<string> UploadAvatarAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[ApiAvatarService] File not found: {filePath}");
            return null;
        }

        string token = PlayerPrefs.GetString("auth_token", "");
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("[ApiAvatarService] No token available");
            return null;
        }

        // 检查文件大小（后端限制 2MB）
        FileInfo fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > 2 * 1024 * 1024)
        {
            Debug.LogWarning($"[ApiAvatarService] File too large: {fileInfo.Length} bytes (max 2MB)");
            return null;
        }

        try
        {
            using (var content = new MultipartFormDataContent())
            {
                // 读取文件字节
                byte[] fileBytes = File.ReadAllBytes(filePath);
                var fileContent = new ByteArrayContent(fileBytes);

                // 设置 Content-Type 根据文件扩展名
                string ext = Path.GetExtension(filePath).ToLower();
                string mimeType = ext switch
                {
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    ".jpg" or ".jpeg" or _ => "image/jpeg"
                };
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

                content.Add(fileContent, "file", Path.GetFileName(filePath));
                content.Headers.Add("Authorization", "Bearer " + token);

                var response = await http.PostAsync($"{BaseUrl}/api/v1/profile/avatar", content);
                string json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var envelope = JsonUtility.FromJson<ApiResponse<AvatarUploadData>>(json);
                    if (envelope != null && envelope.code == 200 && envelope.data != null)
                    {
                        // 拼接完整 URL
                        string fullUrl = BaseUrl + envelope.data.avatar_url;
                        Debug.Log($"[ApiAvatarService] Upload success: {fullUrl}");
                        return fullUrl;
                    }
                }

                Debug.LogWarning($"[ApiAvatarService] Upload failed: {json}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ApiAvatarService] Upload error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 加载头像纹理
    /// </summary>
    public async Task<Texture2D> LoadAvatarTextureAsync(string avatarUrl)
    {
        if (string.IsNullOrEmpty(avatarUrl))
            return null;

        try
        {
            byte[] data = await http.GetByteArrayAsync(avatarUrl);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(data))
            {
                return tex;
            }
            Debug.LogWarning($"[ApiAvatarService] Failed to decode image from {avatarUrl}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ApiAvatarService] Load avatar error: {ex.Message}");
            return null;
        }
    }
}
