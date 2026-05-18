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
            messageText.text = "Login successful";
            messageText.color = Color.green;
            UIManager.Instance.SwitchState(GameState.MainMenu);
        }
        else
        {
            messageText.text = "Invalid username or password";
            messageText.color = Color.red;
        }
    }

    void OnRegisterClicked()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Enter username and password";
            messageText.color = Color.red;
            return;
        }

        if (password.Length < 6)
        {
            messageText.text = "Password must be at least 6 characters";
            messageText.color = Color.red;
            return;
        }

        if (loginService.Register(username, password))
        {
            messageText.text = "Registration successful, please login";
            messageText.color = Color.green;
        }
        else
        {
            messageText.text = "Username already exists";
            messageText.color = Color.red;
        }
    }

    void OnOfflineClicked()
    {
        UIManager.Instance.SwitchState(GameState.MainMenu);
    }
}