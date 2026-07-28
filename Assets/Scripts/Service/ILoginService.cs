using System.Threading.Tasks;

public interface ILoginService
{
    /// <summary>当前存储的 JWT token（只读）</summary>
    string Token { get; }

    /// <summary>当前是否已登录（token 存在且未过期）</summary>
    bool IsLoggedIn { get; }

    /// <summary>当前登录用户名，未登录时为空串</summary>
    string CurrentUser { get; }

    /// <summary>与 CurrentUser 相同，方便面板脚本调用</summary>
    string CurrentUsername { get; }

    /// <summary>注册新用户，成功返回 true</summary>
    Task<bool> RegisterAsync(string username, string password);

    /// <summary>登录，成功返回 true</summary>
    Task<bool> LoginAsync(string username, string password);

    /// <summary>退出登录（清除 token）</summary>
    void Logout();

    /// <summary>获取当前用户信息（包含头像 URL）</summary>
    Task<UserInfo> GetUserInfoAsync();

    /// <summary>获取个人信息（真实姓名、头像、班级等）</summary>
    Task<UserProfile> GetProfileAsync();
}
