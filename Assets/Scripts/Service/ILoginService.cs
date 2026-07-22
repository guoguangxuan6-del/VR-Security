using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoginService
{
    /// <summary>注册新用户，成功返回true，失败返回false</summary>
    bool Register(string username, string password);
    
    /// <summary>登录，成功返回true</summary>
    bool Login(string username, string password);
    
    /// <summary>当前是否已登录</summary>
    bool IsLoggedIn { get; }
    
    /// <summary>当前登录用户名，未登录时为空串</summary>
    string CurrentUser { get; }
    
    /// <summary>与 CurrentUser 相同，方便面板脚本调用</summary>
    string CurrentUsername { get; }
    
    /// <summary>退出登录</summary>
    void Logout();
}