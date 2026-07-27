using UnityEngine;

/// <summary>
/// 核心输入管理器 - 基于 Meta SDK (OVRInput) 进行硬件输入抽象
/// 
/// 已移除 OVRInput.Update() 和 FixedUpdate() 的手动调用，以防与 Oculus 官方内部输入循环冲突，
/// 从而彻底消除了手柄按键被吞、偶尔无法操控的硬件交互瑕疵。
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("VR Hand Anchors")]
    [SerializeField] private Transform leftHandAnchor;
    [SerializeField] private Transform rightHandAnchor;

    public bool IsVRMode { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        IsVRMode = CheckVRHardware();
        Debug.Log($"[InputManager] VR Mode initialized: {IsVRMode}");
    }

    private bool CheckVRHardware()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        return true;
        #else
        try
        {
            return OVRManager.isHmdPresent;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[InputManager] Failed to check OVRManager.isHmdPresent: {ex.Message}. Defaulting to PC Mode.");
            return false;
        }
        #endif
    }

    void Update()
    {
        // 在编辑器中允许按 F12 键在 PC 键盘调试模式和 VR 模式之间快速切换
        if (Application.isEditor && Input.GetKeyDown(KeyCode.F12))
        {
            IsVRMode = !IsVRMode;
            Debug.Log($"[InputManager] Debug Toggle VR Mode: {IsVRMode}");
        }
    }

    // ===== 移动接口 =====
    // WASD 键 / 左手手柄摇杆
    public Vector3 GetMovement()
    {
        if (IsVRMode)
        {
            Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
            return new Vector3(primaryAxis.x, 0, primaryAxis.y);
        }
        else
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            return new Vector3(horizontal, 0, vertical);
        }
    }

    // ===== 视角转向接口 =====
    // 鼠标移动 / 右手手柄摇杆
    public Vector2 GetLook()
    {
        if (IsVRMode)
        {
            return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
        }
        else
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            return new Vector2(mouseX, mouseY);
        }
    }

    // ===== 交互接口 =====
    // E 键或鼠标左键 / 右手手柄 Index Trigger 键
    public bool GetInteract()
    {
        if (IsVRMode)
        {
            return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        }
        else
        {
            return Input.GetKey(KeyCode.E) || Input.GetMouseButton(0);
        }
    }

    public bool GetInteractDown()
    {
        if (IsVRMode)
        {
            return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        }
        else
        {
            return Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0);
        }
    }

    // ===== 抓取接口 =====
    // G 键 / 右手手柄 Hand Grip 键
    public bool GetGrab()
    {
        if (IsVRMode)
        {
            return OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);
        }
        else
        {
            return Input.GetKey(KeyCode.G);
        }
    }

    public bool GetGrabDown()
    {
        if (IsVRMode)
        {
            return OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);
        }
        else
        {
            return Input.GetKeyDown(KeyCode.G);
        }
    }

    // ===== 按压接口 (胸外按压) =====
    // C 键 / 右手手柄 Index Trigger 键
    public bool GetCompression()
    {
        if (IsVRMode)
        {
            return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        }
        else
        {
            return Input.GetKey(KeyCode.C);
        }
    }

    public bool GetCompressionDown()
    {
        if (IsVRMode)
        {
            return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        }
        else
        {
            return Input.GetKeyDown(KeyCode.C);
        }
    }

    // ===== 取消/返回接口 =====
    // ESC 键 / 右手手柄 B 键 (OVRInput.Button.Two)
    public bool GetCancel()
    {
        if (IsVRMode)
        {
            return OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
        }
        else
        {
            return Input.GetKeyDown(KeyCode.Escape);
        }
    }

    // ===== 成绩单显示/隐藏开关 =====
    // Tab 键 / 左手手柄 Y 键 (OVRInput.Button.Four)
    public bool GetToggleScoreReport()
    {
        if (IsVRMode)
        {
            return OVRInput.GetDown(OVRInput.Button.Four, OVRInput.Controller.LTouch);
        }
        else
        {
            return Input.GetKeyDown(KeyCode.Tab);
        }
    }

    // ===== 获取手部位置 =====
    public Transform GetLeftHandTransform()
    {
        return leftHandAnchor;
    }

    public Transform GetRightHandTransform()
    {
        return rightHandAnchor;
    }

    public void SetHandAnchors(Transform left, Transform right)
    {
        leftHandAnchor = left;
        rightHandAnchor = right;
        Debug.Log($"[InputManager] Hand anchors updated. Left: {left.name}, Right: {right.name}");
    }
}
