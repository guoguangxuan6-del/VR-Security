using UnityEngine;

/// <summary>
/// 核心输入管理器 - 基于 Meta SDK (OVRInput, 可选) 进行硬件输入抽象。
/// 无 VR SDK 时自动退化为 PC/键盘/鼠标输入。
/// VR 相关代码统一用 VR_OCULUS 守卫：仅在 Player Settings 定义了该符号且已安装 Oculus 时编译。
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

        // 检测头显是否连接。
        IsVRMode = CheckVRHardware();
        Debug.Log($"[InputManager] VR Mode initialized: {IsVRMode}");
    }

    private bool CheckVRHardware()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return true; // 移动端打包直接启用 VR
#elif VR_OCULUS
        // PC 串流模式下，通过 OVRManager 检测头显是否连接
        try
        {
            return OVRManager.isHmdPresent;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[InputManager] Failed to check OVRManager.isHmdPresent: {ex.Message}. Defaulting to PC Mode.");
            return false;
        }
#else
        return false; // 无 VR SDK，默认 PC 模式
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

#if VR_OCULUS
        // VR 模式下手动更新 OVRInput 状态
        if (IsVRMode)
        {
            OVRInput.Update();
        }
#endif
    }

#if VR_OCULUS
    void FixedUpdate()
    {
        if (IsVRMode)
        {
            OVRInput.FixedUpdate();
        }
    }
#endif

    // ===== 移动接口 =====
    // WASD 键 / 左手手柄摇杆
    public Vector3 GetMovement()
    {
#if VR_OCULUS
        if (IsVRMode)
        {
            Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
            return new Vector3(primaryAxis.x, 0, primaryAxis.y);
        }
#endif
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        return new Vector3(horizontal, 0, vertical);
    }

    // ===== 视角转向接口 =====
    // 鼠标移动 / 右手手柄摇杆
    public Vector2 GetLook()
    {
#if VR_OCULUS
        if (IsVRMode)
        {
            return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
        }
#endif
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        return new Vector2(mouseX, mouseY);
    }

    // ===== 交互接口 =====
    // E 键或鼠标左键 / 右手手柄 Index Trigger 键
    public bool GetInteract()
    {
#if VR_OCULUS
        if (IsVRMode)
        {
            return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        }
#endif
        return Input.GetKey(KeyCode.E) || Input.GetMouseButton(0);
    }

    public bool GetInteractDown()
    {
#if VR_OCULUS
        if (IsVRMode)
        {
            return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        }
#endif
        return Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0);
    }

    // ===== 抓取接口 =====
    // G 键 / 右手手柄 Hand Grip 键
    public bool GetGrab()
    {
#if VR_OCULUS
        if (IsVRMode)
        {
            return OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);
        }
#endif
        return Input.GetKey(KeyCode.G);
    }

    public bool GetGrabDown()
    {
#if VR_OCULUS
        if (IsVRMode)
        {
            return OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);
        }
#endif
        return Input.GetKeyDown(KeyCode.G);
    }

    // ===== 按压接口 (胸外按压) =====
    // C 键 / 右手手柄 Index Trigger 键（按压动作物理判定可另外结合位置变化）
    public bool GetCompression()
    {
#if VR_OCULUS
        if (IsVRMode)
        {
            return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        }
#endif
        return Input.GetKey(KeyCode.C);
    }

    public bool GetCompressionDown()
    {
#if VR_OCULUS
        if (IsVRMode)
        {
            return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        }
#endif
        return Input.GetKeyDown(KeyCode.C);
    }

    // ===== 取消/返回接口 =====
    // ESC 键 / 右手手柄 B 键 (OVRInput.Button.Two)
    public bool GetCancel()
    {
#if VR_OCULUS
        if (IsVRMode)
        {
            return OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
        }
#endif
        return Input.GetKeyDown(KeyCode.Escape);
    }

    // ===== 成绩单显示/隐藏开关 =====
    // Tab 键 / 左手手柄 Y 键 (OVRInput.Button.Four)
    public bool GetToggleScoreReport()
    {
#if VR_OCULUS
        if (IsVRMode)
        {
            return OVRInput.GetDown(OVRInput.Button.Four, OVRInput.Controller.LTouch);
        }
#endif
        return Input.GetKeyDown(KeyCode.Tab);
    }

    // ===== 获取手部位置（VR 模式下抓取或动作位置） =====
    public Transform GetLeftHandTransform()
    {
        return leftHandAnchor;
    }

    public Transform GetRightHandTransform()
    {
        return rightHandAnchor;
    }

    // 用于 OVRCameraRig 初始化时动态绑定手部锚点
    public void SetHandAnchors(Transform left, Transform right)
    {
        leftHandAnchor = left;
        rightHandAnchor = right;
        Debug.Log($"[InputManager] Hand anchors updated. Left: {left.name}, Right: {right.name}");
    }
}
