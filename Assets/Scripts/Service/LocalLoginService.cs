using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class LocalLoginService : ILoginService
{
    private string filePath;
    private List<UserAccount> accounts;
    private string currentUser = "";

    public string Token => "";
    public bool IsLoggedIn => !string.IsNullOrEmpty(currentUser);
    public string CurrentUser => currentUser;
    public string CurrentUsername => currentUser;

    public void Logout()
    {
        currentUser = "";
    }

    public LocalLoginService()
    {
        filePath = Path.Combine(Application.persistentDataPath, "accounts.json");
        LoadAccounts();
    }

    public Task<bool> RegisterAsync(string username, string password)
    {
        if (accounts.Exists(a => a.username == username))
            return Task.FromResult(false);

        accounts.Add(new UserAccount { username = username, password = password });
        SaveAccounts();
        return Task.FromResult(true);
    }

    public Task<bool> LoginAsync(string username, string password)
    {
        var account = accounts.Find(a => a.username == username && a.password == password);
        if (account != null)
        {
            currentUser = username;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<UserInfo> GetUserInfoAsync()
    {
        return Task.FromResult(new UserInfo
        {
            username = currentUser,
            createdAt = "",
            avatarUrl = PlayerPrefs.GetString("avatar_url", "")
        });
    }

    private void LoadAccounts()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            accounts = JsonUtility.FromJson<AccountList>(json)?.accounts ?? new List<UserAccount>();
        }
        else
        {
            accounts = new List<UserAccount>();
        }
    }

    private void SaveAccounts()
    {
        var list = new AccountList { accounts = accounts };
        string json = JsonUtility.ToJson(list, true);
        File.WriteAllText(filePath, json);
    }

    [System.Serializable]
    private class UserAccount
    {
        public string username;
        public string password;
    }

    [System.Serializable]
    private class AccountList
    {
        public List<UserAccount> accounts;
    }
}
