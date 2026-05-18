using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LocalLoginService : ILoginService
{
    private string filePath;
    private List<UserAccount> accounts;
    private string currentUser = "";

    public bool IsLoggedIn => !string.IsNullOrEmpty(currentUser);
    public string CurrentUser => currentUser;

    public LocalLoginService()
    {
        filePath = Path.Combine(Application.persistentDataPath, "accounts.json");
        LoadAccounts();
    }

    public bool Register(string username, string password)
    {
        if (accounts.Exists(a => a.username == username))
            return false;

        accounts.Add(new UserAccount { username = username, password = password });
        SaveAccounts();
        return true;
    }

    public bool Login(string username, string password)
    {
        var account = accounts.Find(a => a.username == username && a.password == password);
        if (account != null)
        {
            currentUser = username;
            return true;
        }
        return false;
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