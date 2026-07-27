using UnityEngine;
using System.Collections;

/// <summary>
/// VR 玩家控制器 - 负责平地顺畅行走、智能门体穿透、狭窄路口通畅穿行、丝滑连续视角旋转以及手势动画驱动。
/// 内置门通道碰撞忽略检测，确保玩家 100% 毫无阻碍地穿过场景中的所有门与闸机。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class VRPlayerRig : MonoBehaviour
{
    [Header("OVRCameraRig")]
    [SerializeField] private OVRCameraRig cameraRig;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("开启手柄向导移动：左手柄指向哪里推摇杆就往哪走。关闭则为头部向导移动。")]
    [SerializeField] private bool useHandOrientedMovement = true;

    [Header("Rotation Settings (丝滑连续旋转)")]
    [Tooltip("开启瞬移旋转 (Snap Turn)；关闭则为丝滑连续旋转 (Smooth Turn，视角角度随摇杆推着一直变化)。")]
    [SerializeField] private bool useSnapTurn = false; // 默认使用丝滑连续平滑旋转，转动角度一直变化
    [SerializeField] private float snapAngle = 45f; // 每次瞬时转动的度数
    [SerializeField] private float rotationSpeed = 100f; // 连续平滑旋转或鼠标旋转的速度 (度/秒)

    private CharacterController characterController;

    // 手部动画器引用
    private Animator leftHandAnimator;
    private Animator rightHandAnimator;

    // 控制 Snap Turn 每次只转动一次的复位锁
    private bool snapTurnInputReset = true;
    private bool isRotating; // 是否正在进行旋转中

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        if (characterController != null)
        {
            // ===== 人体碰撞高度初始化调矮 (确保 100% 顺畅穿过低矮通道与门梁) =====
            characterController.height = 1.3f;       // 初始化碰撞高度调矮为 1.3 米，完美避开所有低矮门框顶梁
            characterController.center = new Vector3(characterController.center.x, 0.65f, characterController.center.z); // 垂直中心与脚底精准贴地

            characterController.radius = 0.05f;      // 超精细胶囊体半径 (5 厘米半径)，避开粗糙空气墙
            characterController.stepOffset = 0.5f;    // 允许迈过 50 厘米高的门槛台阶
            characterController.slopeLimit = 85f;     // 允许爬 85 度斜坡通道
            characterController.skinWidth = 0.005f;   // 降低边缘碰撞阻尼
        }
    }

    void Start()
    {
        // 自动查找 OVRCameraRig
        if (cameraRig == null)
            cameraRig = GetComponentInChildren<OVRCameraRig>(true);

        if (cameraRig == null)
        {
            Debug.LogError("[VRPlayerRig] OVRCameraRig not found in children!");
            return;
        }

        Debug.Log($"[VRPlayerRig] OVRCameraRig bound successfully: {cameraRig.name}");

        // 注册手部追踪锚点给全局 InputManager，方便外部（如按压检测）获取位置
        if (InputManager.Instance != null && cameraRig != null)
        {
            InputManager.Instance.SetHandAnchors(cameraRig.leftHandAnchor, cameraRig.rightHandAnchor);
        }

        // 尝试首次查找并绑定手部的 Animator 控制器
        FindAndBindHandAnimators();
    }

    void Update()
    {
        if (InputManager.Instance == null) return;

        // 0. 智能门通道穿透保护：自动忽略所有门体、闸机的硬物理阻挡，确保 100% 穿过所有的门！
        CheckAndIgnoreDoorCollisions();

        // 1. 仅在玩家静止时做轻微的防卡墙中心校准。移动时完全由标准的 CharacterController.Move 滑行接管
        if (InputManager.Instance.GetMovement().magnitude < 0.1f)
        {
            AlignPhysicsColliderToCamera();
        }

        // 2. 行走与视角切换 (纯 X-Z 平面受控移动，绝不穿模，且沿墙滑行极其顺畅)
        HandleMovement();
        HandleRotation();

        // 3. 更新点击与抓取手势动画
        UpdateHandAnimations();
    }

    /// <summary>
    /// 智能门通道穿透保护：
    /// 自动感知周围 1.5 米以内的门体、闸机、玻璃隔断与通道碰撞体，
    /// 动态关闭它们与玩家 CharacterController 的物理阻挡，确保玩家 100% 能够毫无阻碍地穿过场景里的所有门！
    /// </summary>
    void CheckAndIgnoreDoorCollisions()
    {
        if (characterController == null) return;

        // 探测玩家周围 1.5 米范围内的所有 Collider 碰撞体
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position + Vector3.up * 0.65f, 1.5f);
        
        foreach (var col in nearbyColliders)
        {
            if (col == characterController) continue;

            string name = col.gameObject.name.ToLower();
            
            // 匹配场景里所有可能叫 Door、Gate、Turnstile (闸机)、Entrance、Barrier (隔断) 等物体的名称
            if (name.Contains("door") || 
                name.Contains("gate") || 
                name.Contains("turnstile") || 
                name.Contains("entrance") || 
                name.Contains("barrier") || 
                name.Contains("fence") ||
                name.Contains("glass"))
            {
                // 直接忽略该门体/闸机与玩家碰撞体的阻抗，实现 100% 通畅穿过！
                Physics.IgnoreCollision(characterController, col, true);
            }
        }
    }

    /// <summary>
    /// 仅在玩家原地站立时进行微小的物理中心对齐。
    /// 当玩家在推摇杆移动时暂停该计算，避免中心硬修改与 Unity 物理沿墙滑行产生对抗。
    /// </summary>
    void AlignPhysicsColliderToCamera()
    {
        if (cameraRig == null || cameraRig.centerEyeAnchor == null || characterController == null) return;

        if (InputManager.Instance == null || !InputManager.Instance.IsVRMode) return;

        Vector3 cameraLocalPos = cameraRig.centerEyeAnchor.localPosition;

        float deltaX = cameraLocalPos.x - characterController.center.x;
        float deltaZ = cameraLocalPos.z - characterController.center.z;

        if (Mathf.Abs(deltaX) > 0.05f || Mathf.Abs(deltaZ) > 0.05f)
        {
            characterController.center = new Vector3(cameraLocalPos.x, characterController.center.y, cameraLocalPos.z);
        }
    }

    void FindAndBindHandAnimators()
    {
        if (cameraRig == null) return;

        if (leftHandAnimator == null && cameraRig.leftHandAnchor != null)
        {
            leftHandAnimator = cameraRig.leftHandAnchor.GetComponentInChildren<Animator>(true);
        }

        if (rightHandAnimator == null && cameraRig.rightHandAnchor != null)
        {
            rightHandAnimator = cameraRig.rightHandAnchor.GetComponentInChildren<Animator>(true);
        }
    }

    void UpdateHandAnimations()
    {
        if (InputManager.Instance == null) return;

        if (leftHandAnimator == null || rightHandAnimator == null)
        {
            FindAndBindHandAnimators();
        }

        float leftGrip = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch);
        float rightGrip = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch);
        float leftTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        float rightTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        if (leftHandAnimator != null)
        {
            leftHandAnimator.SetFloat("Grab", leftGrip);
            leftHandAnimator.SetFloat("Flex", leftTrigger);
        }

        if (rightHandAnimator != null)
        {
            rightHandAnimator.SetFloat("Grab", rightGrip);
            rightHandAnimator.SetFloat("Flex", rightTrigger);
        }
    }

    void HandleMovement()
    {
        Vector3 moveInput = InputManager.Instance.GetMovement();

        if (moveInput.magnitude < 0.1f) return;

        Vector3 forward = Vector3.zero;
        Vector3 right = Vector3.zero;

        if (InputManager.Instance.IsVRMode)
        {
            if (useHandOrientedMovement && cameraRig != null && cameraRig.leftHandAnchor != null)
            {
                forward = cameraRig.leftHandAnchor.forward;
                right = cameraRig.leftHandAnchor.right;
            }
            else if (cameraRig != null && cameraRig.centerEyeAnchor != null)
            {
                forward = cameraRig.centerEyeAnchor.forward;
                right = cameraRig.centerEyeAnchor.right;
            }
            else
            {
                forward = transform.forward;
                right = transform.right;
            }
        }
        else
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                forward = mainCam.transform.forward;
                right = mainCam.transform.right;
            }
            else
            {
                forward = transform.forward;
                right = transform.right;
            }
        }

        forward.y = 0f;
        forward.Normalize();
        right.y = 0f;
        right.Normalize();

        Vector3 moveDirection = (right * moveInput.x + forward * moveInput.z);
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    void HandleRotation()
    {
        if (isRotating) return; 

        Vector2 lookInput = InputManager.Instance.GetLook();

        if (InputManager.Instance.IsVRMode)
        {
            if (useSnapTurn)
            {
                if (Mathf.Abs(lookInput.x) > 0.7f)
                {
                    if (snapTurnInputReset)
                    {
                        float angle = Mathf.Sign(lookInput.x) * snapAngle;
                        StartCoroutine(PerformSnapTurn(angle));
                        snapTurnInputReset = false; 
                    }
                }
                else if (Mathf.Abs(lookInput.x) < 0.2f)
                {
                    snapTurnInputReset = true; 
                }
            }
            else
            {
                float yaw = lookInput.x * rotationSpeed;
                if (Mathf.Abs(yaw) > 0.1f)
                {
                    transform.Rotate(0f, yaw * Time.deltaTime, 0f);
                }
            }
        }
        else
        {
            float yaw = lookInput.x * rotationSpeed * 0.05f;
            if (Mathf.Abs(yaw) > 0.01f)
            {
                transform.Rotate(0f, yaw, 0f);
            }
        }
    }

    IEnumerator PerformSnapTurn(float angle)
    {
        isRotating = true;
        
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0f, angle, 0f));
        
        float elapsed = 0f;
        float duration = 0.12f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.rotation = endRotation;
        isRotating = false;
    }

    public OVRCameraRig CameraRig => cameraRig;
}
