using UnityEngine;
using UnityEngine.UI;

public class HomePanel : BasePanel
{
    [SerializeField] private Button loginButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button exitButton;

    void Start()
    {
        AutoBind();
        if (loginButton != null) loginButton.onClick.AddListener(() => UIManager.Instance.NavigateTo("Login"));
        if (settingsButton != null) settingsButton.onClick.AddListener(() => UIManager.Instance.NavigateTo("Settings"));
        if (helpButton != null) helpButton.onClick.AddListener(() => UIManager.Instance.NavigateTo("Help"));
        if (exitButton != null) exitButton.onClick.AddListener(() => UIManager.Instance.OnCancelButtonClicked());
    }

    void AutoBind()
    {
        if (loginButton == null) loginButton = transform.Find("LoginButton")?.GetComponent<Button>();
        if (settingsButton == null) settingsButton = transform.Find("SettingsButton")?.GetComponent<Button>();
        if (helpButton == null) helpButton = transform.Find("HelpButton")?.GetComponent<Button>();
        if (exitButton == null) exitButton = transform.Find("ExitButton")?.GetComponent<Button>();
    }
}
