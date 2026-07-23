using UnityEngine;
using UnityEngine.UI;

public class SceneSelectPanel : BasePanel
{
    [Header("Scene Cards")]
    [SerializeField] private Button subwayCard;
    [SerializeField] private Button hospitalCard;
    [SerializeField] private Button backButton;

    void Start()
    {
        AutoBind();

        if (subwayCard != null)
            subwayCard.onClick.AddListener(OnSubwayClicked);

        if (hospitalCard != null)
            hospitalCard.interactable = false;

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
    }

    void AutoBind()
    {
        if (subwayCard == null)
            subwayCard = transform.Find("SceneCard_Subway")?.GetComponent<Button>();
        if (hospitalCard == null)
            hospitalCard = transform.Find("SceneCard_Hospital")?.GetComponent<Button>();
        if (backButton == null)
            backButton = transform.Find("SceneBackButton")?.GetComponent<Button>();

        Debug.Assert(subwayCard != null, "[SceneSelect] subwayCard not found");
        Debug.Assert(backButton != null, "[SceneSelect] backButton not found");
    }

    void OnSubwayClicked()
    {
        // 加载 CPR 训练场景
        UnityEngine.SceneManagement.SceneManager.LoadScene("Demonstration");
    }

    void OnBackClicked()
    {
        UIManager.Instance.GoBack();
    }
}