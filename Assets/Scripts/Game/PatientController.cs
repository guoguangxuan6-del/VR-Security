using UnityEngine;
using System;

/// <summary>
/// 急救患者控制器 - 管理倒地/抽搐/静止状态机，挂载在人体 FBX 模型上
/// </summary>
[RequireComponent(typeof(Animator))]
public class PatientController : MonoBehaviour
{
    [Header("Animation Timing")]
    [SerializeField] private float fallDuration = 1.5f;
    [SerializeField] private float twitchDuration = 5f;
    [SerializeField] private float autoProgressDelay = 0.3f;

    [Header("Animation Settings")]
    [SerializeField] private float twitchIntensity = 1f;

    private Animator animator;
    private PatientState currentState = PatientState.Standing;
    private bool isAutoProgressing;

    // 状态变化事件
    public event Action<PatientState> OnStateChanged;

    private static readonly int AnimTriggerFall = Animator.StringToHash("TriggerFall");
    private static readonly int AnimTriggerTwitch = Animator.StringToHash("TriggerTwitch");
    private static readonly int AnimSetUnconscious = Animator.StringToHash("SetUnconscious");
    private static readonly int AnimTwitchIntensity = Animator.StringToHash("TwitchIntensity");

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("[PatientController] Animator component not found on patient model!");
            return;
        }
    }

    public void TriggerFall()
    {
        if (currentState != PatientState.Standing) return;

        SetState(PatientState.Falling);
        if (animator != null)
        {
            animator.SetTrigger(AnimTriggerFall);
            animator.SetFloat(AnimTwitchIntensity, twitchIntensity);
        }

        Debug.Log("[PatientController] Patient started falling");
    }

    public void TriggerTwitch()
    {
        if (currentState == PatientState.Twitching) return;

        SetState(PatientState.Twitching);
        if (animator != null)
        {
            animator.SetTrigger(AnimTriggerTwitch);
            animator.SetFloat(AnimTwitchIntensity, twitchIntensity);
        }

        Debug.Log("[PatientController] Patient started twitching");
    }

    public void SetUnconscious()
    {
        if (currentState == PatientState.Unconscious) return;

        SetState(PatientState.Unconscious);
        if (animator != null)
        {
            animator.SetBool(AnimSetUnconscious, true);
        }

        Debug.Log("[PatientController] Patient is now unconscious");
    }

    public void ResetToStanding()
    {
        SetState(PatientState.Standing);
        if (animator != null)
        {
            animator.SetBool(AnimSetUnconscious, false);
            animator.Play("Standing", 0, 0f);
        }

        isAutoProgressing = false;
        Debug.Log("[PatientController] Patient reset to standing");
    }

    private void SetState(PatientState newState)
    {
        if (currentState == newState) return;

        PatientState oldState = currentState;
        currentState = newState;
        OnStateChanged?.Invoke(currentState);

        Debug.Log($"[PatientController] State: {oldState} → {newState}");
    }

    public PatientState CurrentState => currentState;
    public bool IsUnconscious => currentState == PatientState.Unconscious;
}
