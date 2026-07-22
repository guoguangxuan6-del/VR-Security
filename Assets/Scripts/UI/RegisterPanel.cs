using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RegisterPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button backToLoginButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI messageText;

    private ILoginService loginService;

    void Start()
    {
        AutoBind();
        loginService = ServiceLocator.Instance.LoginService;

        registerButton.onClick.AddListener(OnRegisterClicked);
        backToLoginButton.onClick.AddListener(OnBackToLoginClicked);
        closeButton.onClick.AddListener(OnCloseClicked);

        messageText.text = "";
    }

    void AutoBind()
    {
        if (usernameInput == null) usernameInput = transform.Find("UsernameInput")?.GetComponent<TMP_InputField>();
        if (passwordInput == null) passwordInput = transform.Find("PasswordInput")?.GetComponent<TMP_InputField>();
        if (confirmPasswordInput == null) confirmPasswordInput = transform.Find("ConfirmPasswordInput")?.GetComponent<TMP_InputField>();
        if (registerButton == null) registerButton = transform.Find("RegisterButton")?.GetComponent<Button>();
        if (backToLoginButton == null) backToLoginButton = transform.Find("BackToLoginButton")?.GetComponent<Button>();
        if (closeButton == null) closeButton = transform.Find("CloseButton")?.GetComponent<Button>();
        if (messageText == null) messageText = transform.Find("MessageText")?.GetComponent<TextMeshProUGUI>();
    }

    void OnRegisterClicked()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "请输入用户名和密码";
            messageText.color = Color.red;
            return;
        }

        if (password.Length < 6)
        {
            messageText.text = "密码至少6个字符";
            messageText.color = Color.red;
            return;
        }

        if (password != confirmPassword)
        {
            messageText.text = "两次密码不一致";
            messageText.color = Color.red;
            return;
        }

        if (loginService.Register(username, password))
        {
            messageText.text = "注册成功，已自动登录";
            messageText.color = Color.green;
            // 注册成功后自动登录并跳转 Lobby
            loginService.Login(username, password);
            UIManager.Instance.OnLoginSuccess();
        }
        else
        {
            messageText.text = "用户名已存在";
            messageText.color = Color.red;
        }
    }

    void OnBackToLoginClicked()
    {
        UIManager.Instance.NavigateTo("Login");
    }

    void OnCloseClicked()
    {
        UIManager.Instance.GoBack();
    }
}
