using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    [Header("Volume Controls")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider voiceVolumeSlider;
    [SerializeField] private Slider BGMVolumeSlider;

    [Header("Resolution")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Buttons")]
    [SerializeField] private Button resetDataButton;
    [SerializeField] private Button backButton;

    private IScoreRepository scoreRepository;

    void Start()
    {
        AutoBind();
        scoreRepository = ServiceLocator.Instance.ScoreRepository;

        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        voiceVolumeSlider.value = PlayerPrefs.GetFloat("VoiceVolume", 0.8f);
        BGMVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.6f);

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(new System.Collections.Generic.List<string> {
            "1920x1080", "1280x720", "800x600"
        });
        resolutionDropdown.value = PlayerPrefs.GetInt("Resolution", 0);

        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
        BGMVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        resetDataButton.onClick.AddListener(OnResetDataClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }

    void AutoBind()
    {
        if (masterVolumeSlider == null)
            masterVolumeSlider = transform.Find("MasterVolumeSlider")?.GetComponent<Slider>();
        if (voiceVolumeSlider == null)
            voiceVolumeSlider = transform.Find("VoiceVolumeSlider")?.GetComponent<Slider>();
        if (BGMVolumeSlider == null)
            BGMVolumeSlider = transform.Find("BGMVolumeSlider")?.GetComponent<Slider>();
        if (resolutionDropdown == null)
            resolutionDropdown = transform.Find("ResolutionDropdown")?.GetComponent<TMP_Dropdown>();
        if (resetDataButton == null)
            resetDataButton = transform.Find("ResetDataButton")?.GetComponent<Button>();
        if (backButton == null)
            backButton = transform.Find("BackButton")?.GetComponent<Button>();

        Debug.Assert(masterVolumeSlider != null, "[Settings] MasterVolumeSlider not found");
        Debug.Assert(resolutionDropdown != null, "[Settings] ResolutionDropdown not found");
        Debug.Assert(backButton != null, "[Settings] BackButton not found");
    }

    void OnMasterVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        AudioListener.volume = value;
    }

    void OnVoiceVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("VoiceVolume", value);
    }

    void OnBGMVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    void OnResolutionChanged(int index)
    {
        PlayerPrefs.SetInt("Resolution", index);
        switch (index)
        {
            case 0: Screen.SetResolution(1920, 1080, Screen.fullScreen); break;
            case 1: Screen.SetResolution(1280, 720, Screen.fullScreen); break;
            case 2: Screen.SetResolution(800, 600, Screen.fullScreen); break;
        }
    }

    void OnResetDataClicked()
    {
        PlayerPrefs.DeleteKey("MasterVolume");
        PlayerPrefs.DeleteKey("VoiceVolume");
        PlayerPrefs.DeleteKey("BGMVolume");
        PlayerPrefs.DeleteKey("Resolution");

        masterVolumeSlider.value = 0.8f;
        voiceVolumeSlider.value = 0.8f;
        BGMVolumeSlider.value = 0.6f;
        resolutionDropdown.value = 0;

        AudioListener.volume = 0.8f;
        Screen.SetResolution(1920, 1080, Screen.fullScreen);
    }

    void OnBackClicked()
    {
        UIManager.Instance.SwitchState(GameState.MainMenu);
    }
}