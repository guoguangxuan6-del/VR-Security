using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomeMenuPanel : BasePanel
{
    [Header("User Info Bar")]
    [SerializeField] private GameObject userInfoBar;
    [SerializeField] private Image userAvatar;
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private Button logoutButton;

    [Header("Buttons")]
    [SerializeField] private Button btnStudyVideo;
    [SerializeField] private Button btnSkillTraining;
    [SerializeField] private Button btnSettings;
    [SerializeField] private Button btnHelp;
    [SerializeField] private Button exitButton;

    private ILoginService loginService;
    private IAvatarService avatarService;
    private string currentAvatarUrl;
    private const string AvatarUrlKey = "avatar_url";

    void Start()
    {
        AutoBind();
        loginService = ServiceLocator.Instance.LoginService;
        avatarService = ServiceLocator.Instance.AvatarService;

        if (btnStudyVideo != null) btnStudyVideo.onClick.AddListener(() => UIManager.Instance.NavigateTo("StudyVideo"));
        if (btnSkillTraining != null) btnSkillTraining.onClick.AddListener(() => UIManager.Instance.NavigateTo("SkillSelect"));
        if (btnSettings != null) btnSettings.onClick.AddListener(() => UIManager.Instance.NavigateTo("Settings"));
        if (btnHelp != null) btnHelp.onClick.AddListener(() => UIManager.Instance.NavigateTo("Help"));
        if (exitButton != null) exitButton.onClick.AddListener(() => UIManager.Instance.OnCancelButtonClicked());
        if (logoutButton != null) logoutButton.onClick.AddListener(OnLogoutClicked);

        RefreshUserInfo();
    }

    void AutoBind()
    {
        if (userInfoBar == null) userInfoBar = transform.Find("UserInfoBar")?.gameObject;
        if (userAvatar == null) userAvatar = transform.Find("UserInfoBar/Avatar")?.GetComponent<Image>();
        if (nicknameText == null) nicknameText = transform.Find("UserInfoBar/Nickname")?.GetComponent<TextMeshProUGUI>();
        if (logoutButton == null) logoutButton = transform.Find("UserInfoBar/LogoutButton")?.GetComponent<Button>();

        if (btnStudyVideo == null) btnStudyVideo = transform.Find("BtnStudyVideo")?.GetComponent<Button>();
        if (btnSkillTraining == null) btnSkillTraining = transform.Find("BtnSkillTraining")?.GetComponent<Button>();
        if (btnSettings == null) btnSettings = transform.Find("BtnSettings")?.GetComponent<Button>();
        if (btnHelp == null) btnHelp = transform.Find("BtnHelp")?.GetComponent<Button>();
        if (exitButton == null) exitButton = transform.Find("ExitButton")?.GetComponent<Button>();
    }

    void OnLogoutClicked()
    {
        UIManager.Instance.OnLogout();
    }

    /// <summary>
    /// 加载并显示头像
    /// </summary>
    async Task LoadAndSetAvatarImage(string url)
    {
        Texture2D tex = await avatarService.LoadAvatarTextureAsync(url);
        if (tex != null && userAvatar != null)
        {
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
            userAvatar.sprite = sprite;
            userAvatar.preserveAspect = true;
        }
    }

    /// <summary>
    /// 刷新用户信息（登录后调用）
    /// </summary>
    public void RefreshUserInfo()
    {
        if (loginService == null || !loginService.IsLoggedIn)
        {
            if (userInfoBar != null) userInfoBar.SetActive(false);
            return;
        }

        if (userInfoBar != null) userInfoBar.SetActive(true);
        if (nicknameText != null)
        {
            string username = loginService.CurrentUsername;
            nicknameText.text = string.IsNullOrEmpty(username) ? "用户" : username;
        }

        // 获取用户信息并加载头像
        _ = FetchAndDisplayAvatar();
    }

    async Task FetchAndDisplayAvatar()
    {
        // 从后端获取个人信息（含真实姓名和头像）
        var profile = await loginService.GetProfileAsync();
        if (profile != null)
        {
            // 显示真实姓名
            if (nicknameText != null && !string.IsNullOrEmpty(profile.realName))
            {
                nicknameText.text = profile.realName;
            }

            // 加载头像
            if (!string.IsNullOrEmpty(profile.avatar))
            {
                // avatar 是相对路径，拼接完整 URL
                string fullUrl = profile.avatar;
                if (!fullUrl.StartsWith("http"))
                {
                    fullUrl = "http://123.57.30.132:8080" + fullUrl;
                }
                currentAvatarUrl = fullUrl;
                PlayerPrefs.SetString(AvatarUrlKey, fullUrl);
                PlayerPrefs.Save();
                await LoadAndSetAvatarImage(fullUrl);
            }
            return;
        }

        // Fallback 到缓存
        currentAvatarUrl = PlayerPrefs.GetString(AvatarUrlKey, "");
        if (!string.IsNullOrEmpty(currentAvatarUrl))
        {
            await LoadAndSetAvatarImage(currentAvatarUrl);
        }
    }
}
