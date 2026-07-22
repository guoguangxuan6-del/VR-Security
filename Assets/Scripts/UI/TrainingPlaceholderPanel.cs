using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainingPlaceholderPanel : BasePanel
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button backButton;

    void Start()
    {
        AutoBind();

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        if (messageText != null)
            messageText.text = "训练模块开发中";
    }

    void AutoBind()
    {
        if (messageText == null)
            messageText = transform.Find("MessageText")?.GetComponent<TextMeshProUGUI>();
        if (backButton == null)
            backButton = transform.Find("BackButton")?.GetComponent<Button>();

        Debug.Assert(messageText != null, "[TrainingPlaceholder] MessageText not found");
        Debug.Assert(backButton != null, "[TrainingPlaceholder] BackButton not found");
    }

    void OnBackClicked()
    {
        UIManager.Instance.GoBack();
    }
}
