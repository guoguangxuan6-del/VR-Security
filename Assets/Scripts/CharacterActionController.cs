using UnityEngine;

/// <summary>
/// 动作控制器。通过驱动 Animator 状态机来执行 NPC 的动作（如倒地、立正等）。
/// 包含终极防下坠物理锁死保护，自动将刚体设为 Kinematic 状态，彻底杜绝 NPC 因重力穿透场景跌落的问题。
/// </summary>
[RequireComponent(typeof(Animator))]
public class CharacterActionController : MonoBehaviour
{
    private Animator animator;

    [Header("--- 动画参数名配置 ---")]
    public string speedParamName = "Speed";
    public string actionTriggerParamName = "PlayAction";
    public string poseIntParamName = "PoseID";

    void Awake()
    {
        animator = GetComponent<Animator>();

        // 终极防下坠：游戏启动时强行冻结刚体物理，解决所有因为重力、碰撞冲突导致的人体穿模下坠问题！
        // 设为 Kinematic 后，角色绝不可能再跌落，且 100% 允许动画控制其姿态和坐标。
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = GetComponentInChildren<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            Debug.Log($"[动作控制器] 🛡️已自动为 NPC '{gameObject.name}' 开启 Kinematic 并禁用重力，彻底锁死高度，绝不下坠！");
        }
    }

    /// <summary>
    /// 触发指定名字的动画状态
    /// </summary>
    /// <param name="actionName">状态名称，例如 "PainCollapse" 或 "StandUp"</param>
    public void TriggerActionByName(string actionName)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            try
            {
                if (animator.HasState(0, Animator.StringToHash(actionName)))
                {
                    animator.Play(actionName, 0, 0f);
                    return;
                }
            }
            catch {}

            try
            {
                animator.SetTrigger(actionName);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[动作控制器] 尝试以 Trigger 方式触发 {actionName} 时发生兼容警告（可能参数未定义），已忽略: {e.Message}");
            }
        }
    }

    public void SetBoolState(string boolParamName, bool state)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            try
            {
                animator.SetBool(boolParamName, state);
            }
            catch {}
        }
    }

    public void SetMovementSpeed(float speed)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            try
            {
                animator.SetFloat(speedParamName, speed);
            }
            catch {}
        }
    }

    public void SetPose(int poseID)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            try
            {
                animator.SetInteger(poseIntParamName, poseID);
            }
            catch {}
        }
    }
}
