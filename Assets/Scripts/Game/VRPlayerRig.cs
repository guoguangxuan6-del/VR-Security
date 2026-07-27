using UnityEngine;
using System.Collections;

/// <summary>
/// VR 玩家控制器 - 负责平地行走、丝滑连续视角旋转 (Smooth Turn)、物理防卡墙与过门穿透算法。
/// 恢复最原始连续平滑视角转动逻辑：转向时角度持续平滑变化，绝不生硬。
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

    [Header("Rotation Settings (丝滑连续平滑旋转)")]
    [Tooltip("开启瞬移旋转 (Snap Turn)；关闭则为丝滑连续平滑旋转 (Smooth Turn，视角角度随摇杆推着一直变化)。")]
    [SerializeField] private bool useSnapTurn = false; // 默认使用丝滑连续平滑旋转，转动角度一直变化
    [SerializeField] private float snapAngle = 45f; // 每次瞬时转动的度数
    [SerializeField] private float rotationSpeed = 120f; // 连续平滑旋转速度 (度/秒)，角度持续线性平滑变化

    private CharacterController characterController;

    // 手部动画器引用
    private Animator leftHandAnimator;
    private Animator rightHandAnimator;

    // 控制 Snap Turn 每次只转动一次的复位锁
    private bool snapTurnInputReset = true;
    private bool isRotating;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        if (characterController != null)
        {
            characterController.radius = 0.05f;      // 超精细胶囊体半径 (5 厘米)，避开粗糙空气墙
            characterController.stepOffset = 0.5f;    // 允许迈过 50 厘米高的门槛台阶
            characterController.slopeLimit = 85f;     // 允许爬 85 度斜坡通道
            characterController.skinWidth = 0.005f;   // 降低边缘碰撞阻尼
        }
    }

    void Start()
    {
        if (cameraRig == null)
            cameraRig = GetComponentInChildren<OVRCameraRig>(true);

        if (cameraRig == null)
        {
            Debug.LogError("[VRPlayerRig] OVRCameraRig not found in children!");
            return;
        }

        Debug.Log($"[VRPlayerRig] OVRCameraRig bound successfully: {cameraRig.name}");

        if (InputManager.Instance != null && cameraRig != null)
        {
            InputManager.Instance.SetHandAnchors(cameraRig.leftHandAnchor, cameraRig.rightHandAnchor);
        }

        FindAndBindHandAnimators();
    }

    void Update()
    {
        if (InputManager.Instance == null) return;

        // 1. 仅在玩家静止时做轻微的防卡墙中心校准
        if (InputManager.Instance.GetMovement().magnitude < 0.1f)
        {
            AlignPhysicsColliderToCamera();
        }

        // 2. 行走与视角切换 (丝滑连续旋转 Smooth Turn)
        HandleMovement();
        HandleRotation();

        // 3. 更新点击与抓取手势动画
        UpdateHandAnimations();
    }

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

    /// <summary>
    /// 恢复最原始丝滑连续平滑旋转视角逻辑：
    /// 当你推着摇杆向左/右旋转时，视角角度随着摇杆线性持续变化，流畅自然，绝对不生硬。
    /// </summary>
    void HandleRotation()
    {
        if (isRotating) return; 

        Vector2 lookInput = InputManager.Instance.GetLook();

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
            // 丝滑连续平滑旋转 (Smooth Turn) —— 视角随摇杆推着角度持续线性变化
            float yaw = lookInput.x * rotationSpeed * Time.deltaTime;
            if (Mathf.Abs(yaw) > 0.001f)
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
