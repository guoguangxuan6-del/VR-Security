using UnityEngine;
using UnityEngine.UI;

public class HomeMenuPanel : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button btnStudyVideo;
    [SerializeField] private Button btnSkillTraining;
    [SerializeField] private Button btnSettings;
    [SerializeField] private Button btnHelp;

    void Start()
    {
        AutoBind();

        btnStudyVideo.onClick.AddListener(OnStudyVideoClicked);
        btnSkillTraining.onClick.AddListener(OnSkillTrainingClicked);
        btnSettings.onClick.AddListener(OnSettingsClicked);
        btnHelp.onClick.AddListener(OnHelpClicked);
    }

    void AutoBind()
    {
        if (btnStudyVideo == null)
            btnStudyVideo = transform.Find("BtnStudyVideo")?.GetComponent<Button>();
        if (btnSkillTraining == null)
            btnSkillTraining = transform.Find("BtnSkillTraining")?.GetComponent<Button>();
        if (btnSettings == null)
            btnSettings = transform.Find("BtnSettings")?.GetComponent<Button>();
        if (btnHelp == null)
            btnHelp = transform.Find("BtnHelp")?.GetComponent<Button>();

        Debug.Assert(btnStudyVideo != null, "[HomeMenu] BtnStudyVideo not found");
        Debug.Assert(btnSkillTraining != null, "[HomeMenu] BtnSkillTraining not found");
        Debug.Assert(btnSettings != null, "[HomeMenu] BtnSettings not found");
        Debug.Assert(btnHelp != null, "[HomeMenu] BtnHelp not found");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            OnStudyVideoClicked();
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            OnSkillTrainingClicked();
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            OnSettingsClicked();
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            OnHelpClicked();
    }

    void OnStudyVideoClicked()
    {
        Debug.Log("[HomeMenu] Tutorial Video");
        UIManager.Instance.SwitchState(GameState.StudyVideo);
    }

    void OnSkillTrainingClicked()
    {
        Debug.Log("[HomeMenu] Skill Training");
        UIManager.Instance.SwitchState(GameState.SceneSelect);
    }

    void OnSettingsClicked()
    {
        Debug.Log("[HomeMenu] Settings");
        UIManager.Instance.SwitchState(GameState.Settings);
    }

    void OnHelpClicked()
    {
        Debug.Log("[HomeMenu] Help");
        UIManager.Instance.SwitchState(GameState.Help);
    }
}