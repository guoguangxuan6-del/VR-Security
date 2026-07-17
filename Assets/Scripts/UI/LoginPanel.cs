using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginPanel : MonoBehaviour
{
    [Header("UI References")]
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

    void OnEnable()
    {
        if (usernameInput != null) usernameInput.text = "";
        if (passwordInput != null) passwordInput.text = "";
        if (messageText != null) messageText.text = "";
    }

    void AutoBind()
    {
        if (usernameInput == null)
            usernameInput = transform.Find("UsernameInput")?.GetComponent<TMP_InputField>();
        if (passwordInput == null)
            passwordInput = transform.Find("PasswordInput")?.GetComponent<TMP_InputField>();
        if (loginButton == null)
            loginButton = transform.Find("LoginButton")?.GetComponent<Button>();
        if (registerEntryButton == null)
            registerEntryButton = transform.Find("RegisterEntryButton")?.GetComponent<Button>();
        if (closeButton == null)
            closeButton = transform.Find("CloseButton")?.GetComponent<Button>();
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
            messageText.text = "请输入用户名和密码";
            messageText.color = Color.red;
            return;
        }

        if (loginService.Login(username, password))
        {
            messageText.text = "登录成功";
            messageText.color = Color.green;
            UIManager.Instance.HidePopup();
        }
        else
        {
            messageText.text = "用户名或密码错误";
            messageText.color = Color.red;
        }
    }

    void OnRegisterEntryClicked()
    {
        UIManager.Instance.ShowPopup(GameState.Register);
    }

    void OnCloseClicked()
    {
        UIManager.Instance.HidePopup();
    }
}
