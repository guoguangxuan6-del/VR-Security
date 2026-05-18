using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject homeMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private GameObject studyVideoPanel;
    [SerializeField] private GameObject sceneSelectPanel;
    [SerializeField] private GameObject skillSelectPanel;
    [SerializeField] private GameObject scoreReportPanel;

    private GameState currentState;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        AutoBind();
        SwitchState(GameState.Login);
    }

    void AutoBind()
    {
        Canvas loginCanvas = GameObject.Find("LoginCanvas")?.GetComponent<Canvas>();
        if (loginCanvas == null) return;

        if (loginPanel == null)
            loginPanel = loginCanvas.transform.Find("LoginPanel")?.gameObject;
        if (homeMenuPanel == null)
            homeMenuPanel = loginCanvas.transform.Find("HomeMenuPanel")?.gameObject;
        if (settingsPanel == null)
            settingsPanel = loginCanvas.transform.Find("SettingsPanel")?.gameObject;
        if (helpPanel == null)
            helpPanel = loginCanvas.transform.Find("HelpPanel")?.gameObject;
        if (studyVideoPanel == null)
            studyVideoPanel = loginCanvas.transform.Find("StudyVideoPanel")?.gameObject;
        if (sceneSelectPanel == null)
            sceneSelectPanel = loginCanvas.transform.Find("SceneSelectPanel")?.gameObject;
        if (skillSelectPanel == null)
            skillSelectPanel = loginCanvas.transform.Find("SkillSelectPanel")?.gameObject;
        if (scoreReportPanel == null)
            scoreReportPanel = loginCanvas.transform.Find("ScoreReportPanel")?.gameObject;

        Debug.Assert(loginPanel != null, "[UIManager] loginPanel not found");
        Debug.Assert(homeMenuPanel != null, "[UIManager] homeMenuPanel not found");
    }

    public void SwitchState(GameState newState)
    {
        currentState = newState;

        // 隐藏所有面板
        if (loginPanel != null) loginPanel.SetActive(false);
        if (homeMenuPanel != null) homeMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (helpPanel != null) helpPanel.SetActive(false);
        if (studyVideoPanel != null) studyVideoPanel.SetActive(false);
        if (sceneSelectPanel != null) sceneSelectPanel.SetActive(false);
        if (skillSelectPanel != null) skillSelectPanel.SetActive(false);
        if (scoreReportPanel != null) scoreReportPanel.SetActive(false);

        // 显示对应面板
        switch (newState)
        {
            case GameState.Login:
                if (loginPanel != null) loginPanel.SetActive(true);
                break;
            case GameState.MainMenu:
                if (homeMenuPanel != null) homeMenuPanel.SetActive(true);
                break;
            case GameState.Settings:
                if (settingsPanel != null) settingsPanel.SetActive(true);
                break;
            case GameState.Help:
                if (helpPanel != null) helpPanel.SetActive(true);
                break;
            case GameState.StudyVideo:
                if (studyVideoPanel != null) studyVideoPanel.SetActive(true);
                break;
            case GameState.SceneSelect:
                if (sceneSelectPanel != null) sceneSelectPanel.SetActive(true);
                break;
            case GameState.SkillSelect:
                if (skillSelectPanel != null) skillSelectPanel.SetActive(true);
                break;
            case GameState.ScoreReport:
                if (scoreReportPanel != null) scoreReportPanel.SetActive(true);
                break;
            default:
                Debug.LogWarning($"[UIManager] State {newState} not implemented yet");
                break;
        }

        Debug.Log($"[UIManager] Switch to state: {newState}");
    }

    public GameState CurrentState => currentState;
}