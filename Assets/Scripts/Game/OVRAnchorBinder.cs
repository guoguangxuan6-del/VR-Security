using UnityEngine;

/// <summary>
/// 辅助脚本 - 自动查找 OVRCameraRig 的手部追踪锚点并注册给 InputManager
/// 应该挂载在场景中的 OVRCameraRig 物体上
/// </summary>
public class OVRAnchorBinder : MonoBehaviour
{
    [Header("Manual Setup (Optional)")]
    [SerializeField] private Transform leftHandAnchor;
    [SerializeField] private Transform rightHandAnchor;

    void Start()
    {
        if (leftHandAnchor == null)
        {
            // OVRCameraRig 默认结构是 OVRCameraRig -> TrackingSpace -> LeftHandAnchor
            leftHandAnchor = transform.Find("TrackingSpace/LeftHandAnchor");
        }

        if (rightHandAnchor == null)
        {
            rightHandAnchor = transform.Find("TrackingSpace/RightHandAnchor");
        }

        if (leftHandAnchor != null && rightHandAnchor != null)
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.SetHandAnchors(leftHandAnchor, rightHandAnchor);
            }
            else
            {
                Debug.LogWarning("[OVRAnchorBinder] InputManager Instance not found yet. Hand anchors will not be registered.");
            }
        }
        else
        {
            Debug.LogError("[OVRAnchorBinder] Could not automatically locate LeftHandAnchor or RightHandAnchor in OVRCameraRig hierarchy.");
        }
    }
}
