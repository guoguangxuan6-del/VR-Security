using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerEntryButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI messageText;

    private ILoginService loginService;

    void Start()
    {
        AutoBind();
        loginService = ServiceLocator.Instance.LoginService;

        loginButton.onClick.AddListener(OnLoginClicked);
        registerEntryButton.onClick.AddListener(OnRegisterEntryClicked);
        closeButton.onClick.AddListener(OnCloseClicked);

        messageText.text = "";
    }

    void AutoBind()
    {
        if (usernameInput == null) usernameInput = transform.Find("UsernameInput")?.GetComponent<TMP_InputField>();
        if (passwordInput == null) passwordInput = transform.Find("PasswordInput")?.GetComponent<TMP_InputField>();
        if (loginButton == null) loginButton = transform.Find("LoginButton")?.GetComponent<Button>();
        if (registerEntryButton == null) registerEntryButton = transform.Find("RegisterEntryButton")?.GetComponent<Button>();
        if (closeButton == null) closeButton = transform.Find("CloseButton")?.GetComponent<Button>();
        if (messageText == null) messageText = transform.Find("MessageText")?.GetComponent<TextMeshProUGUI>();
    }

    void OnLoginClicked()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "请输入用户名和密码";
            messageText.color = Color.red;
            return;
        }

        if (loginService.Login(username, password))
        {
            messageText.text = "登录成功";
            messageText.color = Color.green;
            // 使用新导航 API：登录成功 → 清栈 → Lobby
            UIManager.Instance.OnLoginSuccess();
        }
        else
        {
            messageText.text = "用户名或密码错误";
            messageText.color = Color.red;
        }
    }

    void OnRegisterEntryClicked()
    {
        UIManager.Instance.NavigateTo("Register");
    }

    void OnCloseClicked()
    {
        UIManager.Instance.GoBack();
    }
}
