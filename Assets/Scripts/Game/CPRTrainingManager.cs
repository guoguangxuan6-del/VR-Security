using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// CPR 心肺复苏按压训练管理器
/// 包含 3 秒启动倒计时、按压黄金区间 (5.0cm - 6.0cm) 判定以及 3D 实时仪表盘 UI。
/// </summary>
public class CPRTrainingManager : MonoBehaviour
{
    public static CPRTrainingManager Instance { get; private set; }

    [Header("Golden Zone Thresholds (黄金区间)")]
    [Tooltip("最佳按压深度下限 (厘米)")]
    [SerializeField] private float minPerfectDepth = 5.0f;
    [Tooltip("最佳按压深度上限 (厘米)")]
    [SerializeField] private float maxPerfectDepth = 6.0f;

    [Header("BPM Target (节奏)")]
    [SerializeField] private float minTargetBPM = 100f;
    [SerializeField] private float maxTargetBPM = 120f;

    [Header("References")]
    [SerializeField] private PatientController patient;

    // 按压统计数据
    public int TotalCompressions { get; private set; }
    public int SuccessfulCompressions { get; private set; }
    public float CurrentBPM { get; private set; }

    private bool isTrainingActive;
    private float lastCompressionTime;
    private float currentSimulatedDepth; // 当前模拟按压深度

    // 3D UI 组件引用
    private GameObject cprCanvasObj;
    private TextMeshProUGUI countdownText;
    private TextMeshProUGUI resultText;
    private TextMeshProUGUI statsText;
    private Image depthBarFill;
    private GameObject meterPanelObj;
    private GameObject countdownPanelObj;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (patient == null)
            patient = FindObjectOfType<PatientController>();

        // 动态构建 3D 按压训练与倒计时 UI
        CreateCPRTrainingUI();
    }

    void Update()
    {
        if (!isTrainingActive) return;

        // 获取按压触发 (PC C键 / VR 右手扳机)
        if (InputManager.Instance != null && InputManager.Instance.GetCompressionDown())
        {
            SimulateCompression();
        }
    }

    /// <summary>
    /// 由情景对话确定按钮唤醒：启动 3 秒倒计时，倒计时结束后开始按压评估
    /// </summary>
    public void StartCountdownAndTraining()
    {
        if (cprCanvasObj != null) cprCanvasObj.SetActive(true);
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        isTrainingActive = false;
        if (meterPanelObj != null) meterPanelObj.SetActive(false);
        if (countdownPanelObj != null) countdownPanelObj.SetActive(true);

        int count = 3;
        while (count > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = count.ToString();
                // 播放弹跳动画
                StartCoroutine(AnimateCountdownText());
            }
            yield return new WaitForSeconds(1.0f);
            count--;
        }

        if (countdownText != null)
        {
            countdownText.text = "<color=#00FF7F>开始按压！</color>";
        }
        yield return new WaitForSeconds(0.6f);

        // 隐藏倒计时，显示 3D 按压仪表盘
        if (countdownPanelObj != null) countdownPanelObj.SetActive(false);
        if (meterPanelObj != null) meterPanelObj.SetActive(true);

        // 正式开启按压评估
        isTrainingActive = true;
        TotalCompressions = 0;
        SuccessfulCompressions = 0;
        UpdateStatsUI();

        Debug.Log("[CPRTrainingManager] CPR Training Active! Target zone: 5.0cm - 6.0cm");
    }

    private IEnumerator AnimateCountdownText()
    {
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1.6f, 1.0f, elapsed / 0.3f);
            if (countdownText != null) countdownText.transform.localScale = Vector3.one * scale;
            yield return null;
        }
    }

    /// <summary>
    /// 模拟/检测一次按压并判定黄金区间
    /// </summary>
    private void SimulateCompression()
    {
        // 随机或模拟按压深度在 4.0 - 6.8cm 之间 (真实按压可以换成手柄向下压的物理位移)
        currentSimulatedDepth = UnityEngine.Random.Range(4.2f, 6.6f);

        // 计算 BPM 频率
        float currentTime = Time.time;
        if (lastCompressionTime > 0)
        {
            float interval = currentTime - lastCompressionTime;
            if (interval > 0.1f)
            {
                CurrentBPM = 60f / interval;
            }
        }
        lastCompressionTime = currentTime;

        TotalCompressions++;

        // 黄金区间判定：只有在 5.0cm - 6.0cm 时才算按压成功！
        bool isSuccess = (currentSimulatedDepth >= minPerfectDepth && currentSimulatedDepth <= maxPerfectDepth);

        if (isSuccess)
        {
            SuccessfulCompressions++;
            ShowResultUI("<color=#00FF7F>★ 按压成功！(Perfect)</color>", new Color(0.0f, 1.0f, 0.5f));
        }
        else if (currentSimulatedDepth < minPerfectDepth)
        {
            ShowResultUI($"<color=#FFD700>按压过浅 ({currentSimulatedDepth:F1}cm)！加重力度</color>", new Color(1.0f, 0.8f, 0.2f));
        }
        else
        {
            ShowResultUI($"<color=#FF4500>按压过深 ({currentSimulatedDepth:F1}cm)！注意力度</color>", new Color(1.0f, 0.3f, 0.3f));
        }

        // 更新深度条进度 (映射 0cm - 8cm)
        if (depthBarFill != null)
        {
            depthBarFill.fillAmount = Mathf.Clamp01(currentSimulatedDepth / 8.0f);
            depthBarFill.color = isSuccess ? new Color(0f, 1f, 0.5f) : new Color(1f, 0.3f, 0.3f);
        }

        UpdateStatsUI();
    }

    private void ShowResultUI(string msg, Color textColor)
    {
        if (resultText != null)
        {
            resultText.text = msg;
        }
    }

    private void UpdateStatsUI()
    {
        if (statsText != null)
        {
            float successRate = TotalCompressions > 0 ? ((float)SuccessfulCompressions / TotalCompressions * 100f) : 0f;
            statsText.text = $"成功按压: <color=#00FF7F>{SuccessfulCompressions}</color> / {TotalCompressions} (成功率: {successRate:F0}%)\n" +
                             $"实时频率: <color=#00E5FF>{CurrentBPM:F0} 次/分</color> (目标: 100-120)";
        }
    }

    /// <summary>
    /// 构建 3D 倒计时与按压仪表盘 Canvas
    /// </summary>
    private void CreateCPRTrainingUI()
    {
        cprCanvasObj = new GameObject("CPRTrainingCanvas");
        if (patient != null) cprCanvasObj.transform.SetParent(patient.transform);
        cprCanvasObj.transform.localPosition = new Vector3(0f, 1.3f, 0.6f); // 位于受害者胸部上方
        cprCanvasObj.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

        Canvas canvas = cprCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        cprCanvasObj.AddComponent<CanvasGroup>();

        // 1. 倒计时面板
        countdownPanelObj = new GameObject("CountdownPanel");
        countdownPanelObj.transform.SetParent(cprCanvasObj.transform, false);

        RectTransform cdRect = countdownPanelObj.AddComponent<RectTransform>();
        cdRect.sizeDelta = new Vector2(400, 300);

        GameObject cdTextObj = new GameObject("CDText");
        cdTextObj.transform.SetParent(countdownPanelObj.transform, false);
        
        RectTransform cdTextRect = cdTextObj.AddComponent<RectTransform>();
        cdTextRect.sizeDelta = cdRect.sizeDelta;

        countdownText = cdTextObj.AddComponent<TextMeshProUGUI>();
        countdownText.text = "3";
        countdownText.fontSize = 120;
        countdownText.fontStyle = FontStyles.Bold;
        countdownText.color = new Color(1.0f, 0.85f, 0.0f); // 亮黄
        countdownText.alignment = TextAlignmentOptions.Center;

        // 2. 按压实时仪表盘面板 (Meter Panel)
        meterPanelObj = new GameObject("MeterPanel");
        meterPanelObj.transform.SetParent(cprCanvasObj.transform, false);

        RectTransform meterRect = meterPanelObj.AddComponent<RectTransform>();
        meterRect.sizeDelta = new Vector2(600, 360);

        Image meterBg = meterPanelObj.AddComponent<Image>();
        meterBg.color = new Color(0.06f, 0.1f, 0.2f, 0.9f); // 深蓝科技感背景

        // 结果反馈标题
        GameObject resObj = new GameObject("ResultText");
        resObj.transform.SetParent(meterPanelObj.transform, false);

        RectTransform resRect = resObj.AddComponent<RectTransform>();
        resRect.sizeDelta = new Vector2(560, 60);
        resRect.anchoredPosition = new Vector2(0, 120);

        resultText = resObj.AddComponent<TextMeshProUGUI>();
        resultText.text = "请准备进行胸外按压 (黄金深度: 5.0 - 6.0 cm)";
        resultText.fontSize = 22;
        resultText.fontStyle = FontStyles.Bold;
        resultText.color = Color.white;
        resultText.alignment = TextAlignmentOptions.Center;

        // 深度进度条背景
        GameObject barBgObj = new GameObject("DepthBarBg");
        barBgObj.transform.SetParent(meterPanelObj.transform, false);

        RectTransform barBgRect = barBgObj.AddComponent<RectTransform>();
        barBgRect.sizeDelta = new Vector2(500, 30);
        barBgRect.anchoredPosition = new Vector2(0, 40);

        Image barBgImg = barBgObj.AddComponent<Image>();
        barBgImg.color = new Color(0.2f, 0.25f, 0.35f, 1f);

        // 深度进度条 Fill
        GameObject barFillObj = new GameObject("DepthBarFill");
        barFillObj.transform.SetParent(barBgObj.transform, false);

        RectTransform fillRect = barFillObj.AddComponent<RectTransform>();
        fillRect.sizeDelta = barBgRect.sizeDelta;

        depthBarFill = barFillObj.AddComponent<Image>();
        depthBarFill.type = Image.Type.Filled;
        depthBarFill.fillMethod = Image.FillMethod.Horizontal;
        depthBarFill.fillAmount = 0.65f;
        depthBarFill.color = new Color(0.0f, 1.0f, 0.5f);

        // 统计数据文本
        GameObject statsObj = new GameObject("StatsText");
        statsObj.transform.SetParent(meterPanelObj.transform, false);

        RectTransform statsRect = statsObj.AddComponent<RectTransform>();
        statsRect.sizeDelta = new Vector2(560, 100);
        statsRect.anchoredPosition = new Vector2(0, -90);

        statsText = statsObj.AddComponent<TextMeshProUGUI>();
        statsText.text = "成功按压: 0 / 0 (成功率: 0%)\n按压频率: 0 次/分 (目标: 100-120)";
        statsText.fontSize = 20;
        statsText.color = new Color(0.9f, 0.95f, 1.0f);
        statsText.alignment = TextAlignmentOptions.Center;

        meterPanelObj.SetActive(false);
        cprCanvasObj.SetActive(false);
    }
}
