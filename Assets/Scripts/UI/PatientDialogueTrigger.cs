using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// NPC 接近情景对话触发器 - 专为 VR 设备优化的三重无死角点击响应
/// 1. 支持手柄绿色射线指向点击 (OVRRaycaster/GraphicRaycaster)
/// 2. 支持手部物理伸过去直接触碰按钮 (Direct Touch Collider)
/// 3. 支持按下右手 Index Trigger / E 键按键直连快捷确认
/// </summary>
public class PatientDialogueTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("触发对话的临界距离 (米)")]
    [SerializeField] private float triggerDistance = 2.5f;
    [Tooltip("患者控制器")]
    [SerializeField] private PatientController patient;

    [Header("UI Aesthetics & Styling")]
    [SerializeField] private Vector3 dialogueOffset = new Vector3(0f, 1.6f, 0f); // 浮空高度

    private Transform playerTransform;
    private GameObject dialogueCanvasObj;
    private CanvasGroup canvasGroup;
    private bool hasTriggered;
    private bool isDialogueActive;

    void Start()
    {
        if (patient == null)
            patient = GetComponent<PatientController>();

        FindPlayer();
        CreateDialogueUI();
    }

    void Update()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        float distance = Vector3.Distance(playerTransform.position, transform.position);

        if (!hasTriggered && distance <= triggerDistance)
        {
            TriggerDialogue();
        }

        // 3D 对话框 Billboard 朝向：始终柔和面向玩家眼睛
        if (isDialogueActive && dialogueCanvasObj != null)
        {
            Vector3 targetDir = dialogueCanvasObj.transform.position - playerTransform.position;
            targetDir.y = 0;
            if (targetDir != Vector3.zero)
            {
                dialogueCanvasObj.transform.rotation = Quaternion.LookRotation(targetDir);
            }

            // ===== 三重触发保障：按键直连确认 =====
            // 当对话框激活时，按右手 Index Trigger / E 键也可以直接确认开始急救
            if (InputManager.Instance != null && InputManager.Instance.GetInteractDown())
            {
                OnStartCPRClicked();
            }
        }
    }

    void FindPlayer()
    {
        var playerRig = FindObjectOfType<VRPlayerRig>();
        if (playerRig != null)
        {
            playerTransform = playerRig.transform;
        }
        else
        {
            var mainCam = Camera.main;
            if (mainCam != null) playerTransform = mainCam.transform;
        }
    }

    public void TriggerDialogue()
    {
        hasTriggered = true;
        isDialogueActive = true;

        if (dialogueCanvasObj != null)
        {
            dialogueCanvasObj.SetActive(true);
            StartCoroutine(FadeInUI());
        }

        Debug.Log("[PatientDialogueTrigger] Medical Expert Scenario Triggered!");
    }

    void CreateDialogueUI()
    {
        dialogueCanvasObj = new GameObject("PatientDialogueCanvas");
        dialogueCanvasObj.transform.SetParent(transform);
        dialogueCanvasObj.transform.localPosition = dialogueOffset;
        dialogueCanvasObj.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

        Canvas canvas = dialogueCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        canvasGroup = dialogueCanvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // 兼容 Meta Oculus 射线检测与普通 UI Raycaster
        dialogueCanvasObj.AddComponent<GraphicRaycaster>();
        System.Type ovrRaycasterType = System.Type.GetType("OVRRaycaster");
        if (ovrRaycasterType != null)
        {
            dialogueCanvasObj.AddComponent(ovrRaycasterType);
        }

        // 背景主面板 (Glassmorphism 玻璃拟态深蓝卡片)
        GameObject panelObj = new GameObject("BackgroundPanel");
        panelObj.transform.SetParent(dialogueCanvasObj.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(700, 480);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.09f, 0.18f, 0.92f);

        // 标头
        GameObject headerObj = new GameObject("HeaderTitle");
        headerObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform headerRect = headerObj.AddComponent<RectTransform>();
        headerRect.sizeDelta = new Vector2(640, 50);
        headerRect.anchoredPosition = new Vector2(0, 185);

        TextMeshProUGUI headerText = headerObj.AddComponent<TextMeshProUGUI>();
        headerText.text = "🚨 紧急医疗情境 · 专家判定";
        headerText.fontSize = 28;
        headerText.fontStyle = FontStyles.Bold;
        headerText.color = new Color(1.0f, 0.35f, 0.35f);
        headerText.alignment = TextAlignmentOptions.Center;

        // 主体文本
        GameObject bodyObj = new GameObject("BodyText");
        bodyObj.transform.SetParent(panelObj.transform, false);

        RectTransform bodyRect = bodyObj.AddComponent<RectTransform>();
        bodyRect.sizeDelta = new Vector2(620, 240);
        bodyRect.anchoredPosition = new Vector2(0, 30);

        TextMeshProUGUI bodyText = bodyObj.AddComponent<TextMeshProUGUI>();
        bodyText.text = "<color=#FFD700>【身份：医学专家】</color>\n\n" +
                        "在繁忙的地铁站内，面前这名乘客突然痛苦抽搐并重重倒地，目前已完全<color=#FF4500>失去意识</color>！\n\n" +
                        "情况万分危急！作为在场的专业医疗人员，你必须立刻对其展开<color=#00FF7F>意识评估与 CPR 心肺复苏按压</color>！";
        bodyText.fontSize = 22;
        bodyText.lineSpacing = 12;
        bodyText.color = new Color(0.92f, 0.95f, 1.0f);
        bodyText.alignment = TextAlignmentOptions.Left;

        // 确认按钮
        GameObject btnObj = new GameObject("StartCPRButton");
        btnObj.transform.SetParent(panelObj.transform, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(360, 65);
        btnRect.anchoredPosition = new Vector2(0, -170);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.0f, 0.75f, 0.45f, 1.0f);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(OnStartCPRClicked);

        // ===== 三重触发保障：物理手部触碰 Collider =====
        BoxCollider btnBox = btnObj.AddComponent<BoxCollider>();
        btnBox.size = new Vector3(360f, 65f, 50f); // 给予 3D 深度厚的触碰包围盒
        btnBox.isTrigger = true;
        
        // 挂载手触碰脚本
        var touchTrigger = btnObj.AddComponent<ButtonTouchTrigger>();
        touchTrigger.onTouched = OnStartCPRClicked;

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);

        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.sizeDelta = btnRect.sizeDelta;
        btnTextRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "⚡ 开始急救评估与按压";
        btnText.fontSize = 24;
        btnText.fontStyle = FontStyles.Bold;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;

        dialogueCanvasObj.SetActive(false);
    }

    /// <summary>
    /// 点击/触碰/按键确认后的回调
    /// </summary>
    public void OnStartCPRClicked()
    {
        if (!isDialogueActive) return;

        Debug.Log("[PatientDialogueTrigger] User confirmed start CPR button!");

        if (patient != null)
        {
            patient.TriggerFall();
        }

        // 唤醒 3 秒倒计时与按压黄金区间评测
        if (CPRTrainingManager.Instance != null)
        {
            CPRTrainingManager.Instance.StartCountdownAndTraining();
        }

        StartCoroutine(FadeOutUI());
    }

    IEnumerator FadeInUI()
    {
        float elapsed = 0f;
        while (elapsed < 0.4f)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / 0.4f);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    IEnumerator FadeOutUI()
    {
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / 0.3f));
            yield return null;
        }
        canvasGroup.alpha = 0f;
        dialogueCanvasObj.SetActive(false);
        isDialogueActive = false;
    }
}

/// <summary>
/// 辅助物理触碰类：当 VR 手部物理碰触到 3D 按钮时唤醒点击
/// </summary>
public class ButtonTouchTrigger : MonoBehaviour
{
    public System.Action onTouched;

    void OnTriggerEnter(Collider other)
    {
        // 当碰撞物体名称包含 Hand、Controller 或者是追踪锚点时触发
        string n = other.name.ToLower();
        if (n.Contains("hand") || n.Contains("controller") || n.Contains("touch") || n.Contains("anchor"))
        {
            onTouched?.Invoke();
        }
    }
}
