using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button offlineButton;
    [SerializeField] private TextMeshProUGUI messageText;

    private ILoginService loginService;

    void Start()
    {
        AutoBind();
        loginService = ServiceLocator.Instance.LoginService;

        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
        offlineButton.onClick.AddListener(OnOfflineClicked);

        messageText.text = "";
    }

    void AutoBind()
    {
        if (usernameInput == null)
            usernameInput = transform.Find("UsernameInput")?.GetComponent<TMP_InputField>();
        if (passwordInput == null)
            passwordInput = transform.Find("PasswordInput")?.GetComponent<TMP_InputField>();
        if (loginButton == null)
            loginButton = transform.Find("LoginButton")?.GetComponent<Button>();
        if (registerButton == null)
            registerButton = transform.Find("RegisterButton")?.GetComponent<Button>();
        if (offlineButton == null)
            offlineButton = transform.Find("OfflineButton")?.GetComponent<Button>();
        if (messageText == null)
            messageText = transform.Find("MessageText")?.GetComponent<TextMeshProUGUI>();

        Debug.Assert(usernameInput != null, "[LoginPanel] UsernameInput not found");
        Debug.Assert(passwordInput != null, "[LoginPanel] PasswordInput not found");
        Debug.Assert(loginButton != null, "[LoginPanel] LoginButton not found");
        Debug.Assert(messageText != null, "[LoginPanel] MessageText not found");
    }

    void OnLoginClicked()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Enter username and password";
            messageText.color = Color.red;
            return;
        }

        if (loginService.Login(username, password))
        {
            messageText.text = "登录成功";
            messageText.color = Color.green;
            UIManager.Instance.SwitchState(GameState.MainMenu);
        }
        else
        {
            messageText.text = "用户名或密码无效";
            messageText.color = Color.red;
        }
    }

    void OnRegisterClicked()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "请输入用户名和密码";
            messageText.color = Color.red;
            return;
        }

        if (password.Length < 6)
        {
            messageText.text = "密码必须至少包含6个字符";
            messageText.color = Color.red;
            return;
        }

        if (loginService.Register(username, password))
        {
            messageText.text = "注册成功，请登录";
            messageText.color = Color.green;
        }
        else
        {
            messageText.text = "用户名已存在";
            messageText.color = Color.red;
        }
    }

    void OnOfflineClicked()
    {
        UIManager.Instance.SwitchState(GameState.MainMenu);
    }
}