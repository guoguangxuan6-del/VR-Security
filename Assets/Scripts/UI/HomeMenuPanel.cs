using UnityEngine;
using UnityEngine.UI;

public class HomeMenuPanel : BasePanel
{
    [Header("Buttons")]
    [SerializeField] private Button btnStudyVideo;
    [SerializeField] private Button btnSkillTraining;
    [SerializeField] private Button btnSettings;
    [SerializeField] private Button btnHelp;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button backButton;

    private ILoginService loginService;

    void Start()
    {
        AutoBind();
        loginService = ServiceLocator.Instance.LoginService;

        btnStudyVideo.onClick.AddListener(OnStudyVideoClicked);
        btnSkillTraining.onClick.AddListener(OnSkillTrainingClicked);
        btnSettings.onClick.AddListener(OnSettingsClicked);
        btnHelp.onClick.AddListener(OnHelpClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    public override void OnEnter(object data)
    {
        RefreshOfflineState();
    }

    void OnEnable()
    {
        RefreshOfflineState();
    }

    void AutoBind()
    {
        if (btnStudyVideo == null)
            btnStudyVideo = transform.Find("BtnStudyVideo")?.GetComponent<Button>();
        if (btnSkillTraining == null)
            btnSkillTraining = transform.Find("BtnSkillTraining")?.GetComponent<Button>();
        if (btnSettings == null)
            btnSettings = transform.Find("BtnSettings")?.GetComponent<Button>();
        if (btnHelp == null)
            btnHelp = transform.Find("BtnHelp")?.GetComponent<Button>();
        if (exitButton == null)
            exitButton = transform.Find("ExitButton")?.GetComponent<Button>();
        if (backButton == null)
            backButton = transform.Find("BackButton")?.GetComponent<Button>();

        Debug.Assert(btnStudyVideo != null, "[HomeMenu] BtnStudyVideo not found");
        Debug.Assert(btnSkillTraining != null, "[HomeMenu] BtnSkillTraining not found");
        Debug.Assert(btnSettings != null, "[HomeMenu] BtnSettings not found");
        Debug.Assert(btnHelp != null, "[HomeMenu] BtnHelp not found");
    }

    void RefreshOfflineState()
    {
        if (loginService == null) return;

        bool isLoggedIn = loginService.IsLoggedIn;
        btnSkillTraining.interactable = isLoggedIn;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            OnStudyVideoClicked();
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            OnSkillTrainingClicked();
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            OnSettingsClicked();
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            OnHelpClicked();
    }

    void OnStudyVideoClicked()
    {
        UIManager.Instance.SwitchState(GameState.StudyVideo);
    }

    void OnSkillTrainingClicked()
    {
        if (loginService != null && !loginService.IsLoggedIn)
        {
            Debug.Log("[HomeMenu] Training requires login");
            return;
        }
        UIManager.Instance.SwitchState(GameState.SceneSelect);
    }

    void OnSettingsClicked()
    {
        UIManager.Instance.SwitchState(GameState.Settings);
    }

    void OnHelpClicked()
    {
        UIManager.Instance.SwitchState(GameState.Help);
    }

    void OnExitClicked()
    {
        OnBack();
    }

    void OnBackClicked()
    {
        OnBack();
    }
}
