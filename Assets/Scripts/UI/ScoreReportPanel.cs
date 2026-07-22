using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreReportPanel : BasePanel
{
    [Header("Score Display")]
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI detailContent;
    [SerializeField] private TextMeshProUGUI adviceContent;

    [Header("Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button backToMenuButton;

    private IScoreRepository scoreRepository;

    void Start()
    {
        AutoBind();
        scoreRepository = ServiceLocator.Instance.ScoreRepository;

        if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
        if (backToMenuButton != null) backToMenuButton.onClick.AddListener(OnBackToMenuClicked);

        LoadLatestScore();
    }

    void AutoBind()
    {
        if (totalScoreText == null)
            totalScoreText = transform.Find("TotalScoreText")?.GetComponent<TextMeshProUGUI>();
        if (detailContent == null)
            detailContent = transform.Find("ScoreDetailPanel/DetailContent")?.GetComponent<TextMeshProUGUI>();
        if (adviceContent == null)
            adviceContent = transform.Find("AdviceContent")?.GetComponent<TextMeshProUGUI>();
        if (retryButton == null)
            retryButton = transform.Find("RetryButton")?.GetComponent<Button>();
        if (backToMenuButton == null)
            backToMenuButton = transform.Find("BackToMenuButton")?.GetComponent<Button>();

        Debug.Assert(totalScoreText != null, "[ScoreReport] TotalScoreText not found");
        Debug.Assert(detailContent != null, "[ScoreReport] DetailContent not found");
        Debug.Assert(adviceContent != null, "[ScoreReport] AdviceContent not found");
        Debug.Assert(retryButton != null, "[ScoreReport] RetryButton not found");
        Debug.Assert(backToMenuButton != null, "[ScoreReport] BackToMenuButton not found");
    }

    void LoadLatestScore()
    {
        ScoreData latest = scoreRepository.GetLatestScore();
        if (latest != null)
        {
            totalScoreText.text = $"Total: {latest.totalScore:F1}";
            detailContent.text = FormatDetails(latest);
            adviceContent.text = GenerateAdvice(latest);
        }
        else
        {
            totalScoreText.text = "Total: --";
            detailContent.text = "No training records yet";
            adviceContent.text = "Complete training for personalized advice";
        }
    }

    string FormatDetails(ScoreData data)
    {
        if (data.stepDetails == null || data.stepDetails.Count == 0)
            return "No detail data";

        string result = $"Scene: {data.scene} | Skill: {data.skill}\n";
        result += $"Depth: {data.compressionDepthAvg:F1} cm\n";
        result += $"Rate: {data.compressionRateAvg:F0} /min\n";
        result += $"Errors: {data.errorCount}\n";

        foreach (var step in data.stepDetails)
        {
            result += $"{step.stepName}: {step.score}";
            if (!string.IsNullOrEmpty(step.comment))
                result += $" ({step.comment})";
            result += "\n";
        }
        return result.TrimEnd('\n');
    }

    string GenerateAdvice(ScoreData data)
    {
        if (data.totalScore >= 90)
            return "Excellent! Keep it up.";
        if (data.totalScore >= 70)
            return "Good. Focus on depth and rate consistency.";
        if (data.totalScore >= 50)
            return "Needs improvement. Focus on rhythm and sequence.";
        return "Review the tutorial video and practice basics.";
    }

    void OnRetryClicked()
    {
        UIManager.Instance.SwitchState(GameState.SceneSelect);
    }

    void OnBackToMenuClicked()
    {
        UIManager.Instance.GoBack();
    }
}