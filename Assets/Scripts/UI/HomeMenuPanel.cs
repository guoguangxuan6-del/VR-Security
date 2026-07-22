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

    void Start()
    {
        AutoBind();
        loginService = ServiceLocator.Instance.LoginService;

        // 使用新导航 API 绑定按钮
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
    }
}
