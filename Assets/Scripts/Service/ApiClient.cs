using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// API 响应信封（与后端 {code, message, data} 结构一致）
/// </summary>
[Serializable]
public class ApiResponse<T>
{
    public int code;
    public string message;
    public T data;
}

/// <summary>
/// API 客户端基类，使用 HttpClient 绕过 Unity 的平台限制
/// </summary>
public class ApiClient
{
    protected const string BaseUrl = "http://123.57.30.132:8080";

    private const string TokenKey = "auth_token";
    private const string ExpiresAtKey = "auth_expires_at";

    protected static readonly HttpClient http = new HttpClient();

    /// <summary>
    /// 存储的 token（登录/注册后自动保存）
    /// </summary>
    public static string Token
    {
        get => PlayerPrefs.GetString(TokenKey, "");
        protected set => PlayerPrefs.SetString(TokenKey, value);
    }

    /// <summary>
    /// Token 过期时间（毫秒时间戳）
    /// </summary>
    public static long ExpiresAt
    {
        get => long.Parse(PlayerPrefs.GetString(ExpiresAtKey, "0"));
        protected set => PlayerPrefs.SetString(ExpiresAtKey, value.ToString());
    }

    /// <summary>
    /// Token 是否有效（未过期且非空）
    /// </summary>
    public static bool IsTokenValid()
    {
        if (string.IsNullOrEmpty(Token)) return false;
        long now = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
        return now < ExpiresAt;
    }

    /// <summary>
    /// 清除 token
    /// </summary>
    public static void ClearToken()
    {
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.DeleteKey(ExpiresAtKey);
    }

    /// <summary>
    /// 保存 token 响应
    /// </summary>
    protected static void SaveToken(string token, long expiresAt)
    {
        PlayerPrefs.SetString(TokenKey, token);
        PlayerPrefs.SetString(ExpiresAtKey, expiresAt.ToString());
    }

    /// <summary>
    /// 异步 GET 请求
    /// </summary>
    protected static async Task<ApiResponse<T>> GetAsync<T>(string path, bool requireAuth = false)
    {
        string url = BaseUrl + path;
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("Accept", "application/json");
        if (requireAuth && !string.IsNullOrEmpty(Token))
            req.Headers.Add("Authorization", "Bearer " + Token);

        var resp = await http.SendAsync(req);
        string json = await resp.Content.ReadAsStringAsync();
        return ParseResponse<T>(json, resp.IsSuccessStatusCode);
    }

    /// <summary>
    /// 异步 POST 请求（JSON body）
    /// </summary>
    protected static async Task<ApiResponse<T>> PostAsync<T>(string path, object body, bool requireAuth = false)
    {
        string url = BaseUrl + path;
        string jsonBody = body != null ? JsonUtility.ToJson(body) : "{}";

        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };
        req.Headers.Add("Accept", "application/json");
        if (requireAuth && !string.IsNullOrEmpty(Token))
            req.Headers.Add("Authorization", "Bearer " + Token);

        var resp = await http.SendAsync(req);
        string json = await resp.Content.ReadAsStringAsync();
        return ParseResponse<T>(json, resp.IsSuccessStatusCode);
    }

    /// <summary>
    /// 解析响应
    /// </summary>
    private static ApiResponse<T> ParseResponse<T>(string json, bool isSuccess)
    {
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                var response = JsonUtility.FromJson<ApiResponse<T>>(json);
                if (response != null) return response;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ApiClient] JSON parse error: {ex.Message}");
            }
        }

        return new ApiResponse<T>
        {
            code = -1,
            message = isSuccess ? "Parse failed" : "Request failed",
            data = default
        };
    }
}
