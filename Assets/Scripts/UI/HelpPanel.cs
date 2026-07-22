using UnityEngine;
using UnityEngine.UI;

public class HelpPanel : BasePanel
{
    [Header("Mode Panels")]
    [SerializeField] private GameObject keyboardHelp;
    [SerializeField] private GameObject vrHelp;
    [SerializeField] private Button backButton;

    void Start()
    {
        AutoBind();
        backButton.onClick.AddListener(OnBackClicked);
        DetectInputDevice();
    }

    void AutoBind()
    {
        if (keyboardHelp == null)
            keyboardHelp = transform.Find("KeyboardHelp")?.gameObject;
        if (vrHelp == null)
            vrHelp = transform.Find("VRHelp")?.gameObject;
        if (backButton == null)
            backButton = transform.Find("HelpBackButton")?.GetComponent<Button>();

        Debug.Assert(keyboardHelp != null, "[HelpPanel] KeyboardHelp not found");
        Debug.Assert(backButton != null, "[HelpPanel] HelpBackButton not found");
    }

    void DetectInputDevice()
    {
        bool isVR = false;

#if UNITY_2020_1_OR_NEWER
        isVR = UnityEngine.XR.XRSettings.isDeviceActive;
#endif

        if (keyboardHelp != null) keyboardHelp.SetActive(!isVR);
        if (vrHelp != null) vrHelp.SetActive(isVR);
    }

    void OnBackClicked()
    {
        UIManager.Instance.GoBack();
    }
}