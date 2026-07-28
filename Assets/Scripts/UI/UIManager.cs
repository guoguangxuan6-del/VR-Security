using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Hologram Terminal")]
    [SerializeField] private HologramTerminal hologramTerminal;

    // 导航状态（单一状态源）
    private Stack<string> panelHistory = new Stack<string>();
    private string currentPanelName = "Home";
    private bool isAnimating = false;

    // 自动收集所有面板引用
    private Dictionary<string, GameObject> panelDict = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (hologramTerminal == null)
            hologramTerminal = FindObjectOfType<HologramTerminal>();

        // 自动收集面板
        CollectPanels();

        // 根据登录状态显示初始面板
        ShowInitialPanel();
    }

    /// <summary>
    /// 自动收集 TerminalCanvas 下所有 Panel
    /// </summary>
    void CollectPanels()
    {
        panelDict.Clear();
        if (hologramTerminal == null) return;
        var canvas = hologramTerminal.transform.Find("TerminalCanvas");
        if (canvas == null) return;

        foreach (Transform child in canvas)
        {
            string name = child.name;
            if (name.EndsWith("Panel"))
            {
                string key = name.Substring(0, name.Length - 5); // 去掉 "Panel"
                panelDict[key] = child.gameObject;
            }
        }
    }

    void ShowInitialPanel()
    {
        var loginService = ServiceLocator.Instance?.LoginService;
        if (loginService != null && loginService.IsLoggedIn)
            ResetTo("Lobby");
        else
            ResetTo("Home");
    }

    // ═══════════════════════════════════════════════════════════════
    // 导航方法
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 普通跳转（记录历史）
    /// </summary>
    public void NavigateTo(string panelName)
    {
        if (isAnimating) return;
        if (panelName == currentPanelName) return;

        panelHistory.Push(currentPanelName);
        currentPanelName = panelName;
        ShowPanel(panelName);
    }

    /// <summary>
    /// 返回上一级
    /// </summary>
    public void GoBack()
    {
        if (isAnimating) return;

        if (panelHistory.Count > 0)
        {
            string targetPanel = panelHistory.Pop();
            
            // 防御性检查：如果目标面板是 Lobby 但用户未登录，改为 Home
            if (targetPanel == "Lobby")
            {
                var loginService = ServiceLocator.Instance?.LoginService;
                if (loginService == null || !loginService.IsLoggedIn)
                {
                    targetPanel = "Home";
                    panelHistory.Clear();
                }
            }
            
            currentPanelName = targetPanel;
            ShowPanel(currentPanelName);
        }
        else
        {
            // 无历史，退出终端
            hologramTerminal.DeactivateTerminal();
            currentPanelName = "Home";
        }
    }

    /// <summary>
    /// 清栈跳转（登录成功/退出登录）
    /// </summary>
    public void ResetTo(string panelName)
    {
        if (isAnimating) return;

        panelHistory.Clear();
        currentPanelName = panelName;
        ShowPanel(panelName);
    }

    // ═══════════════════════════════════════════════════════════════
    // 面板切换
    // ═══════════════════════════════════════════════════════════════

    void ShowPanel(string panelName)
    {
        if (hologramTerminal == null) return;
        isAnimating = true;
        hologramTerminal.ShowPanel(panelName);
    }

    /// <summary>
    /// 由 HologramTerminal 在动画完成时调用
    /// </summary>
    public void OnPanelAnimationComplete()
    {
        isAnimating = false;
    }

    // ═══════════════════════════════════════════════════════════════
    // 事件处理器（由 HologramTerminal 调用）
    // ═══════════════════════════════════════════════════════════════

    public void OnLoginSuccess()
    {
        ResetTo("Lobby");
        // 刷新用户信息
        var lobby = GetPanel("Lobby")?.GetComponent<HomeMenuPanel>();
        lobby?.RefreshUserInfo();
    }

    public void OnLogout()
    {
        var loginService = ServiceLocator.Instance?.LoginService;
        loginService?.Logout();
        ResetTo("Home");
    }

    public void OnCancelButtonClicked()
    {
        hologramTerminal.DeactivateTerminal();
    }

    // ═══════════════════════════════════════════════════════════════
    // 兼容旧接口
    // ═══════════════════════════════════════════════════════════════

    public void SwitchState(GameState newState)
    {
        switch (newState)
        {
            case GameState.Login: NavigateTo("Login"); break;
            case GameState.Register: NavigateTo("Register"); break;
            case GameState.Settings: NavigateTo("Settings"); break;
            case GameState.Help: NavigateTo("Help"); break;
            case GameState.StudyVideo: NavigateTo("StudyVideo"); break;
            case GameState.SceneSelect: NavigateTo("SceneSelect"); break;
            case GameState.SkillSelect: NavigateTo("SkillSelect"); break;
            case GameState.ScoreReport: NavigateTo("ScoreReport"); break;
            case GameState.Training: LoadTrainingScene(); break;
            case GameState.Home: NavigateTo("Home"); break;
            case GameState.Lobby: NavigateTo("Lobby"); break;
        }
    }

    public void ShowPopup(GameState popupState) => SwitchState(popupState);
    public void HidePopup() => GoBack();
    public void SwapPopup(GameState popupState) => SwitchState(popupState);
    public void OnBackButtonClicked() => GoBack();

    // ═══════════════════════════════════════════════════════════════
    // 工具方法
    // ═══════════════════════════════════════════════════════════════

    public GameObject GetPanel(string name)
    {
        return panelDict.ContainsKey(name) ? panelDict[name] : null;
    }

    public string CurrentPanelName => currentPanelName;

    void LoadTrainingScene()
    {
        SceneManager.LoadScene("CPRTraining");
    }
}
