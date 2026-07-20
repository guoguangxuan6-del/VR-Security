using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Fullscreen Panels")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private GameObject studyVideoPanel;
    [SerializeField] private GameObject sceneSelectPanel;
    [SerializeField] private GameObject skillSelectPanel;
    [SerializeField] private GameObject scoreReportPanel;
    [SerializeField] private GameObject trainingPlaceholderPanel;

    [Header("Popup Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;

    private Stack<GameState> stateHistory = new Stack<GameState>();
    private GameState currentState;
    private bool hasPopup;
    private bool isAnimating;
    private bool hasNavigated;

    const float FULLSCREEN_FADE_DURATION = 0.125f; // 一半时间做fade out, 一半做fade in, 总共0.25s
    const float POPUP_OPEN_DURATION = 0.2f;
    const float POPUP_CLOSE_DURATION = 0.15f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        AutoBind();
        SwitchState(GameState.Home);
    }

    void AutoBind()
    {
        Canvas loginCanvas = GameObject.Find("LoginCanvas")?.GetComponent<Canvas>();
        if (loginCanvas == null) return;

        if (homePanel == null)
            homePanel = loginCanvas.transform.Find("HomePanel")?.gameObject;
        if (lobbyPanel == null)
            lobbyPanel = loginCanvas.transform.Find("LobbyPanel")?.gameObject;
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
        if (trainingPlaceholderPanel == null)
            trainingPlaceholderPanel = loginCanvas.transform.Find("TrainingPlaceholderPanel")?.gameObject;
        if (loginPanel == null)
            loginPanel = loginCanvas.transform.Find("LoginPanel")?.gameObject;
        if (registerPanel == null)
            registerPanel = loginCanvas.transform.Find("RegisterPanel")?.gameObject;

        Debug.Assert(homePanel != null, "[UIManager] homePanel not found");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (hasPopup)
                HidePopup();
            else
                GoBack();
        }
    }

    public void SwitchState(GameState newState)
    {
        if (newState == GameState.Login || newState == GameState.Register)
        {
            ShowPopup(newState);
            return;
        }
        if (newState == GameState.Training)
        {
            SceneManager.LoadScene("CPRTraining");
            return;
        }
        if (isAnimating) return;

        // OnExit current panel
        GameObject oldPanel = GetActivePanel();
        if (oldPanel != null && oldPanel.TryGetComponent<BasePanel>(out var oldBp))
            oldBp.OnExit();

        if (hasNavigated && currentState != newState)
            stateHistory.Push(currentState);
        hasNavigated = true;
        currentState = newState;

        StartCoroutine(TransitionFullScreen(newState, oldPanel));
    }

    public void GoBack()
    {
        if (isAnimating) return;

        if (stateHistory.Count > 0)
        {
            GameObject oldPanel = GetActivePanel();
            if (oldPanel != null && oldPanel.TryGetComponent<BasePanel>(out var oldBp))
                oldBp.OnExit();

            GameState prev = stateHistory.Pop();
            currentState = prev;
            StartCoroutine(TransitionFullScreen(prev, oldPanel));
        }
    }

    public void ShowPopup(GameState popupState)
    {
        if (isAnimating) return;

        hasPopup = true;
        GameObject panel = popupState == GameState.Login ? loginPanel : registerPanel;
        if (panel != null)
            StartCoroutine(PopupOpen(panel));
    }

    public void SwapPopup(GameState popupState)
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        ShowPopup(popupState);
    }

    public void HidePopup()
    {
        if (isAnimating) return;

        GameObject popup = loginPanel != null && loginPanel.activeSelf ? loginPanel : registerPanel;
        if (popup != null)
            StartCoroutine(PopupClose(popup));
        else
            FinishHidePopup();
    }

    void FinishHidePopup()
    {
        hasPopup = false;
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);

        GameObject activePanel = GetActivePanel();
        if (activePanel != null && activePanel.TryGetComponent<HomePanel>(out var hp))
            hp.RefreshUI();
    }

    IEnumerator TransitionFullScreen(GameState targetState, GameObject oldPanel)
    {
        isAnimating = true;

        // Fade out old panel
        if (oldPanel != null)
            yield return FadePanel(oldPanel, 1f, 0f, FULLSCREEN_FADE_DURATION);

        HideAllPanels();

        // Show and fade in new panel
        GameObject newPanel = GetPanelForState(targetState);
        if (newPanel != null)
        {
            newPanel.SetActive(true);
            SetPanelAlpha(newPanel, 0f);
            yield return FadePanel(newPanel, 0f, 1f, FULLSCREEN_FADE_DURATION);

            if (newPanel.TryGetComponent<BasePanel>(out var bp))
                bp.OnEnter(null);
        }

        isAnimating = false;
    }

    IEnumerator PopupOpen(GameObject panel)
    {
        isAnimating = true;
        panel.SetActive(true);

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

        RectTransform rt = panel.GetComponent<RectTransform>();
        if (rt != null) rt.localScale = Vector3.one * 0.5f;

        float elapsed = 0f;
        while (elapsed < POPUP_OPEN_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / POPUP_OPEN_DURATION;
            float eased = 1f - (1f - t) * (1f - t); // ease-out

            if (cg != null) cg.alpha = eased;
            if (rt != null) rt.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, eased);

            yield return null;
        }

        if (cg != null) cg.alpha = 1f;
        if (rt != null) rt.localScale = Vector3.one;

        isAnimating = false;
    }

    IEnumerator PopupClose(GameObject panel)
    {
        isAnimating = true;

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        RectTransform rt = panel.GetComponent<RectTransform>();

        float elapsed = 0f;
        while (elapsed < POPUP_CLOSE_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / POPUP_CLOSE_DURATION;
            float eased = t * t; // ease-in

            if (cg != null) cg.alpha = 1f - eased;
            if (rt != null) rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.8f, eased);

            yield return null;
        }

        FinishHidePopup();
        isAnimating = false;
    }

    IEnumerator FadePanel(GameObject panel, float from, float to, float duration)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    void SetPanelAlpha(GameObject panel, float alpha)
    {
        CanvasGroup cg = panel?.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = alpha;
    }

    void HideAllPanels()
    {
        if (homePanel != null) homePanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (helpPanel != null) helpPanel.SetActive(false);
        if (studyVideoPanel != null) studyVideoPanel.SetActive(false);
        if (sceneSelectPanel != null) sceneSelectPanel.SetActive(false);
        if (skillSelectPanel != null) skillSelectPanel.SetActive(false);
        if (scoreReportPanel != null) scoreReportPanel.SetActive(false);
        if (trainingPlaceholderPanel != null) trainingPlaceholderPanel.SetActive(false);
    }

    GameObject GetPanelForState(GameState state)
    {
        return state switch
        {
            GameState.Home => homePanel,
            GameState.Lobby => lobbyPanel,
            GameState.Settings => settingsPanel,
            GameState.Help => helpPanel,
            GameState.StudyVideo => studyVideoPanel,
            GameState.SceneSelect => sceneSelectPanel,
            GameState.SkillSelect => skillSelectPanel,
            GameState.ScoreReport => scoreReportPanel,
            GameState.Training => trainingPlaceholderPanel,
            _ => null
        };
    }

    GameObject GetActivePanel()
    {
        return GetPanelForState(currentState);
    }

    public GameState CurrentState => currentState;
}
