using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomePanel : BasePanel
{
    [Header("UI References")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private Button offlineButton;
    [SerializeField] private Button exitButton;

    private ILoginService loginService;

    public override void OnEnter(object data)
    {
        RefreshUI();
    }

    void Start()
    {
        AutoBind();
        loginService = ServiceLocator.Instance.LoginService;

        actionButton.onClick.AddListener(OnActionClicked);
        offlineButton.onClick.AddListener(OnOfflineClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    void OnEnable()
    {
        RefreshUI();
    }

    void AutoBind()
    {
        if (actionButton == null)
            actionButton = transform.Find("ActionButton")?.GetComponent<Button>();
        if (actionButtonText == null)
            actionButtonText = transform.Find("ActionButton/Text")?.GetComponent<TextMeshProUGUI>();
        if (offlineButton == null)
            offlineButton = transform.Find("OfflineButton")?.GetComponent<Button>();
        if (exitButton == null)
            exitButton = transform.Find("ExitButton")?.GetComponent<Button>();

        Debug.Assert(actionButton != null, "[HomePanel] ActionButton not found");
        Debug.Assert(offlineButton != null, "[HomePanel] OfflineButton not found");
        Debug.Assert(exitButton != null, "[HomePanel] ExitButton not found");
    }

    public void RefreshUI()
    {
        if (loginService == null) return;

        if (loginService.IsLoggedIn)
            actionButtonText.text = "进入大厅";
        else
            actionButtonText.text = "点击登录";
    }

    void OnActionClicked()
    {
        if (loginService.IsLoggedIn)
            UIManager.Instance.SwitchState(GameState.Lobby);
        else
            UIManager.Instance.ShowPopup(GameState.Login);
    }

    void OnOfflineClicked()
    {
        UIManager.Instance.SwitchState(GameState.Lobby);
    }

    void OnExitClicked()
    {
        Application.Quit();
    }
}
