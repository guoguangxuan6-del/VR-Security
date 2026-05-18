using UnityEngine;
using UnityEngine.UI;

public class SkillSelectPanel : MonoBehaviour
{
    [Header("Skill Cards")]
    [SerializeField] private Button cprCard;
    [SerializeField] private Button aedCard;
    [SerializeField] private Button backButton;

    void Start()
    {
        AutoBind();

        if (cprCard != null) cprCard.onClick.AddListener(OnCPRClicked);
        if (aedCard != null) aedCard.onClick.AddListener(OnAEDClicked);
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
    }

    void AutoBind()
    {
        if (cprCard == null)
            cprCard = transform.Find("SkillCard_CPR")?.GetComponent<Button>();
        if (aedCard == null)
            aedCard = transform.Find("SkillCard_AED")?.GetComponent<Button>();
        if (backButton == null)
            backButton = transform.Find("SkillBackButton")?.GetComponent<Button>();

        Debug.Assert(cprCard != null, "[SkillSelect] CPR card not found");
        Debug.Assert(aedCard != null, "[SkillSelect] AED card not found");
        Debug.Assert(backButton != null, "[SkillSelect] BackButton not found");
    }

    void OnCPRClicked()
    {
        UIManager.Instance.SwitchState(GameState.Training);
    }

    void OnAEDClicked()
    {
        UIManager.Instance.SwitchState(GameState.Training);
    }

    void OnBackClicked()
    {
        UIManager.Instance.SwitchState(GameState.SceneSelect);
    }
}