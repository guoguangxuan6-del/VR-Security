using System;
using System.Threading.Tasks;
using UnityEngine;

public class ApiLoginService : ApiClient, ILoginService
{
    private string currentUser = "";

    public string Token => PlayerPrefs.GetString("auth_token", "");
    public bool IsLoggedIn => IsTokenValid();
    public string CurrentUser => currentUser;
    public string CurrentUsername => currentUser;

    public async Task<bool> RegisterAsync(string username, string password)
    {
        var body = new AuthRequest { username = username, password = password };
        var resp = await PostAsync<AuthData>("/api/v1/auth/register", body);

        if (resp != null && resp.code == 200 && resp.data != null)
        {
            SaveToken(resp.data.token, resp.data.expiresAt);
            currentUser = username;
            return true;
        }
        Debug.LogWarning($"[ApiLoginService] Register failed: {resp?.message}");
        return false;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var body = new AuthRequest { username = username, password = password };
        var resp = await PostAsync<AuthData>("/api/v1/auth/login", body);

        if (resp != null && resp.code == 200 && resp.data != null)
        {
            SaveToken(resp.data.token, resp.data.expiresAt);
            currentUser = username;
            return true;
        }
        Debug.LogWarning($"[ApiLoginService] Login failed: {resp?.message}");
        return false;
    }

    public void Logout()
    {
        ClearToken();
        currentUser = "";
    }

    public async Task<UserProfile> GetProfileAsync()
    {
        var resp = await GetAsync<UserProfile>("/api/v1/profile", requireAuth: true);

        if (resp != null && resp.code == 200 && resp.data != null)
        {
            return resp.data;
        }
        Debug.LogWarning($"[ApiLoginService] GetProfile failed: {resp?.message}");
        return null;
    }

    public async Task<UserInfo> GetUserInfoAsync()
    {
        var resp = await GetAsync<UserInfo>("/api/v1/user/info", requireAuth: true);

        if (resp != null && resp.code == 200 && resp.data != null)
        {
            return resp.data;
        }
        Debug.LogWarning($"[ApiLoginService] GetUserInfo failed: {resp?.message}");
        return null;
    }

    [Serializable]
    private class AuthRequest
    {
        public string username;
        public string password;
    }
}
