using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class HologramTerminal : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float promptShowDistance = 5f;
    [SerializeField] private float interactDistance = 3f;

    [Header("References")]
    [SerializeField] private Transform floatingIcon;
    [SerializeField] private Transform terminalBase;
    [SerializeField] private Renderer beamRenderer;
    [SerializeField] private Canvas promptCanvas;
    [SerializeField] private TMPro.TextMeshProUGUI promptText;
    [SerializeField] private GameObject arrowMarker;

    [Header("Events")]
    public UnityEvent OnTerminalActivated;
    public UnityEvent OnTerminalDeactivated;

    private Vector3 startPosition;
    private Vector3 arrowBasePos;
    private Material baseMaterial;
    private Material beamMaterial;
    private bool isActivated = false;
    private bool isPlayerNearby = false;
    private float baseEmissionIntensity = 1f;
    private Transform playerTransform;
    private PlayerController playerController;
    private Camera mainCamera;
    private GameObject currentPanel;
    private Dictionary<string, GameObject> panelDict = new Dictionary<string, GameObject>();
    private bool isAnimating = false;

    [Header("Transition")]
    [SerializeField] private float fadeDuration = 0.2f;

    void Start()
    {
        startPosition = transform.position;
        mainCamera = Camera.main;

        if (mainCamera != null)
        {
            playerTransform = mainCamera.transform.root;
            playerController = playerTransform?.GetComponent<PlayerController>();
        }

        if (terminalBase != null)
        {
            baseMaterial = terminalBase.GetComponent<Renderer>()?.material;
            if (baseMaterial != null)
                baseEmissionIntensity = baseMaterial.GetFloat("_EmissiveIntensity");
        }

        if (beamRenderer != null)
            beamMaterial = beamRenderer.material;

        if (promptText != null)
        {
            promptText.text = "按 E 键交互";
        }

        if (promptCanvas != null)
        {
            promptCanvas.gameObject.SetActive(false);
            promptCanvas.worldCamera = mainCamera;
        }

        var terminalCanvas = transform.Find("TerminalCanvas")?.GetComponent<Canvas>();
        if (terminalCanvas != null && mainCamera != null)
            terminalCanvas.worldCamera = mainCamera;

        if (arrowMarker != null)
        {
            arrowMarker.SetActive(true);
            arrowBasePos = arrowMarker.transform.position;
        }

        InitPanelDict();
    }

    void InitPanelDict()
    {
        panelDict.Clear();
        var canvas = transform.Find("TerminalCanvas");
        if (canvas == null) return;
        foreach (Transform child in canvas)
        {
            string name = child.name;
            if (name.EndsWith("Panel"))
                panelDict[name.Substring(0, name.Length - 5)] = child.gameObject;
        }
    }

    void Update()
    {
        if (mainCamera == null) return;

        float distance = Vector3.Distance(mainCamera.transform.position, transform.position);

        if (!isActivated)
        {
            AnimateFloat();
            AnimatePulse();
            AnimateBeamFlicker();
            AnimateArrow();

            if (distance < promptShowDistance && !isPlayerNearby)
            {
                isPlayerNearby = true;
                ShowPrompt();
            }
            else if (distance >= promptShowDistance && isPlayerNearby)
            {
                isPlayerNearby = false;
                HidePrompt();
            }

            if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
                ActivateTerminal();
        }
    }

    void AnimateFloat()
    {
        if (floatingIcon == null) return;
        float newY = startPosition.y + Mathf.Sin(Time.time * 1.5f) * 0.1f;
        Vector3 newPos = floatingIcon.position;
        newPos.y = newY;
        floatingIcon.position = newPos;
    }

    void AnimatePulse()
    {
        if (baseMaterial == null) return;
        float pulse = Mathf.Sin(Time.time * 2f) * 0.5f + 0.5f;
        float intensity = Mathf.Lerp(baseEmissionIntensity * 0.8f, baseEmissionIntensity * 1.2f, pulse);
        baseMaterial.SetFloat("_EmissiveIntensity", intensity);
    }

    void AnimateArrow()
    {
        if (arrowMarker == null) return;
        float newY = arrowBasePos.y + Mathf.Sin(Time.time * 1.8f) * 0.4f;
        Vector3 p = arrowMarker.transform.position;
        p.y = newY;
        arrowMarker.transform.position = p;
    }

    void AnimateBeamFlicker()
    {
        if (beamMaterial == null) return;
        float flicker = Mathf.Sin(Time.time * 3f) * 0.15f + 0.85f;
        Color currentColor = beamMaterial.GetColor("_BaseColor");
        currentColor.a = flicker * 0.3f;
        beamMaterial.SetColor("_BaseColor", currentColor);
    }

    void ShowPrompt()
    {
        if (promptCanvas != null)
            promptCanvas.gameObject.SetActive(true);
    }

    void HidePrompt()
    {
        if (promptCanvas != null)
        {
            promptCanvas.gameObject.SetActive(false);
            promptCanvas.worldCamera = mainCamera;
        }
    }

    public void ActivateTerminal()
    {
        if (isActivated) return;
        isActivated = true;
        HidePrompt();
        if (arrowMarker != null) arrowMarker.SetActive(false);
        if (playerController != null) playerController.SetCanMove(false);
        OnTerminalActivated?.Invoke();
    }

    public void DeactivateTerminal()
    {
        if (!isActivated) return;
        isActivated = false;
        if (arrowMarker != null) arrowMarker.SetActive(true);
        if (playerController != null) playerController.SetCanMove(true);
        OnTerminalDeactivated?.Invoke();
    }

    public void ShowPanel(string panelName)
    {
        if (isAnimating) return;
        StartCoroutine(ShowPanelRoutine(panelName));
    }

    IEnumerator ShowPanelRoutine(string panelName)
    {
        isAnimating = true;

        // 淡出当前面板
        if (currentPanel != null)
        {
            var cg = currentPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                float t = 0;
                while (t < fadeDuration)
                {
                    t += Time.deltaTime;
                    cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
                    yield return null;
                }
            }
        }

        // 隐藏所有面板
        foreach (var kvp in panelDict)
        {
            if (kvp.Value != null)
                kvp.Value.SetActive(false);
        }

        // 显示新面板
        currentPanel = null;
        if (panelDict.ContainsKey(panelName))
        {
            currentPanel = panelDict[panelName];
            if (currentPanel != null)
            {
                currentPanel.SetActive(true);
                var cg = currentPanel.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 0f;
                    float t = 0;
                    while (t < fadeDuration)
                    {
                        t += Time.deltaTime;
                        cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
                        yield return null;
                    }
                    cg.alpha = 1f;
                }
            }
        }

        isAnimating = false;
        
        // 通知 UIManager 动画完成
        UIManager.Instance?.OnPanelAnimationComplete();
    }

    public string GetCurrentPanelName() => currentPanel != null ? currentPanel.name.Replace("Panel","") : null;
    public bool IsActivated => isActivated;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, promptShowDistance);
    }
}
