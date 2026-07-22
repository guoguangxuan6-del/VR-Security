using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : BasePanel
{
    [Header("Volume Controls")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider voiceVolumeSlider;
    [SerializeField] private Slider BGMVolumeSlider;

    [Header("Resolution")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Buttons")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button resetDataButton;
    [SerializeField] private Button backButton;

    private IScoreRepository scoreRepository;

    private float originalMaster;
    private float originalVoice;
    private float originalBGM;
    private int originalResolution;

    private bool hasApplyCancel;
    private bool hasChanges;

    void Start()
    {
        AutoBind();
        scoreRepository = ServiceLocator.Instance.ScoreRepository;

        hasApplyCancel = applyButton != null && cancelButton != null;

        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
        BGMVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        resetDataButton.onClick.AddListener(OnResetDataClicked);
        backButton.onClick.AddListener(OnBackClicked);

        if (applyButton != null)
            applyButton.onClick.AddListener(OnApplyClicked);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
    }

    public override void OnEnter(object data)
    {
        CaptureOriginals();
        LoadFromPlayerPrefs();
        SetApplyCancelVisible(false);
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
        if (applyButton == null)
            applyButton = transform.Find("ApplyButton")?.GetComponent<Button>();
        if (cancelButton == null)
            cancelButton = transform.Find("CancelButton")?.GetComponent<Button>();
        if (resetDataButton == null)
            resetDataButton = transform.Find("ResetDataButton")?.GetComponent<Button>();
        if (backButton == null)
            backButton = transform.Find("BackButton")?.GetComponent<Button>();

        Debug.Assert(masterVolumeSlider != null, "[Settings] MasterVolumeSlider not found");
        Debug.Assert(resolutionDropdown != null, "[Settings] ResolutionDropdown not found");
        Debug.Assert(backButton != null, "[Settings] BackButton not found");
    }

    void CaptureOriginals()
    {
        originalMaster = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        originalVoice = PlayerPrefs.GetFloat("VoiceVolume", 0.8f);
        originalBGM = PlayerPrefs.GetFloat("BGMVolume", 0.6f);
        originalResolution = PlayerPrefs.GetInt("Resolution", 0);
    }

    void LoadFromPlayerPrefs()
    {
        masterVolumeSlider.SetValueWithoutNotify(originalMaster);
        voiceVolumeSlider.SetValueWithoutNotify(originalVoice);
        BGMVolumeSlider.SetValueWithoutNotify(originalBGM);

        if (resolutionDropdown.options.Count == 0)
        {
            resolutionDropdown.AddOptions(new System.Collections.Generic.List<string> {
                "1920x1080", "1280x720", "800x600"
            });
        }
        resolutionDropdown.SetValueWithoutNotify(originalResolution);
    }

    void MarkChanged()
    {
        if (!hasApplyCancel) return;
        hasChanges = true;
        SetApplyCancelVisible(true);
    }

    void SetApplyCancelVisible(bool visible)
    {
        if (applyButton != null) applyButton.gameObject.SetActive(visible);
        if (cancelButton != null) cancelButton.gameObject.SetActive(visible);
    }

    void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
        if (!hasApplyCancel) PlayerPrefs.SetFloat("MasterVolume", value);
        MarkChanged();
    }

    void OnVoiceVolumeChanged(float value)
    {
        if (!hasApplyCancel) PlayerPrefs.SetFloat("VoiceVolume", value);
        MarkChanged();
    }

    void OnBGMVolumeChanged(float value)
    {
        if (!hasApplyCancel) PlayerPrefs.SetFloat("BGMVolume", value);
        MarkChanged();
    }

    void OnResolutionChanged(int index)
    {
        ApplyResolution(index);
        if (!hasApplyCancel) PlayerPrefs.SetInt("Resolution", index);
        MarkChanged();
    }

    void OnApplyClicked()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("VoiceVolume", voiceVolumeSlider.value);
        PlayerPrefs.SetFloat("BGMVolume", BGMVolumeSlider.value);
        PlayerPrefs.SetInt("Resolution", resolutionDropdown.value);
        CaptureOriginals();
        hasChanges = false;
        SetApplyCancelVisible(false);
    }

    void OnCancelClicked()
    {
        RestoreOriginals();
        hasChanges = false;
        SetApplyCancelVisible(false);
    }

    void OnResetDataClicked()
    {
        masterVolumeSlider.value = 0.8f;
        voiceVolumeSlider.value = 0.8f;
        BGMVolumeSlider.value = 0.6f;
        resolutionDropdown.value = 0;

        AudioListener.volume = 0.8f;
        ApplyResolution(0);

        if (!hasApplyCancel)
        {
            PlayerPrefs.DeleteKey("MasterVolume");
            PlayerPrefs.DeleteKey("VoiceVolume");
            PlayerPrefs.DeleteKey("BGMVolume");
            PlayerPrefs.DeleteKey("Resolution");
        }
    }

    void OnBackClicked()
    {
        if (hasApplyCancel && hasChanges)
            RestoreOriginals();
        UIManager.Instance.GoBack();
    }

    void RestoreOriginals()
    {
        masterVolumeSlider.SetValueWithoutNotify(originalMaster);
        voiceVolumeSlider.SetValueWithoutNotify(originalVoice);
        BGMVolumeSlider.SetValueWithoutNotify(originalBGM);
        resolutionDropdown.SetValueWithoutNotify(originalResolution);

        AudioListener.volume = originalMaster;
        ApplyResolution(originalResolution);
    }

    void ApplyResolution(int index)
    {
        switch (index)
        {
            case 0: Screen.SetResolution(1920, 1080, Screen.fullScreen); break;
            case 1: Screen.SetResolution(1280, 720, Screen.fullScreen); break;
            case 2: Screen.SetResolution(800, 600, Screen.fullScreen); break;
        }
    }
}
