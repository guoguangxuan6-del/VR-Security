using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC 受伤与救援事件管理器。
/// 用于串联倒地肢体动作与面部痛苦表情，方便与 VR 交互（手柄按键、射线点击等）事件对接。
/// </summary>
public class NPCInjuryEventController : MonoBehaviour
{
    private FacialExpressionController facialController;
    private CharacterActionController actionController;

    [Header("--- 动画与表情参数名设置 (必须对应 Animator 中的 Trigger) ---")]
    [Tooltip("痛苦倒地动作在 Animator 中的 Trigger 触发器名称")]
    public string collapseTriggerName = "PainCollapse";
    [Tooltip("起身动作在 Animator 中的 Trigger 触发器名称")]
    public string standUpTriggerName = "StandUp";
    [Tooltip("用于标记受伤状态的 Bool 参数名 (可选)")]
    public string injuredBoolName = "IsInjured";
    [Tooltip("痛苦表情的名称")]
    public string painExpressionName = "Pain";

    [Header("--- 调试按键 ---")]
    [Tooltip("是否允许在运行时用键盘测试事件 (K键倒地，R键救起)")]
    public bool enableKeyboardDebug = true;

    private bool isInjured = false;

    void Start()
    {
        facialController = GetComponent<FacialExpressionController>();
        actionController = GetComponent<CharacterActionController>();

        if (facialController == null) facialController = gameObject.AddComponent<FacialExpressionController>();
        if (actionController == null) actionController = gameObject.AddComponent<CharacterActionController>();

        // 自动为表情控制器注册痛苦表情贴图
        AutoRegisterPainTexture();
    }

    void Update()
    {
        if (enableKeyboardDebug)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                TriggerPainAndCollapse();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                TriggerRescueSuccess();
            }
        }
    }

    /// <summary>
    /// 【核心接口】NPC 痛苦倒地：切换痛苦表情，停止自动眨眼，并触发 Animator 的倒地动画
    /// </summary>
    public void TriggerPainAndCollapse()
    {
        if (isInjured) return;
        isInjured = true;

        Debug.Log("[NPC事件] ⚠️NPC发病！触发 Animator 中的 PainCollapse 状态，并切换痛苦表情。");

        if (actionController != null)
        {
            actionController.TriggerActionByName(collapseTriggerName);
            actionController.SetBoolState(injuredBoolName, true);
        }

        if (facialController != null)
        {
            facialController.SetTextureExpression(painExpressionName, pauseBlink: true);
        }
    }

    /// <summary>
    /// 【核心接口】NPC 救护成功：恢复正常表情与眨眼，并触发 Animator 起身动画
    /// </summary>
    public void TriggerRescueSuccess()
    {
        if (!isInjured) return;
        isInjured = false;

        Debug.Log("[NPC事件] ✨救护成功！触发 Animator 中的 StandUp 状态，并恢复面部眨眼。");

        if (actionController != null)
        {
            actionController.SetBoolState(injuredBoolName, false);
            actionController.TriggerActionByName(standUpTriggerName); 
        }

        if (facialController != null)
        {
            facialController.ResetToDefaultTexture();
        }
    }

    private void AutoRegisterPainTexture()
    {
        if (facialController == null) return;

        bool hasPain = false;
        foreach (var expr in facialController.textureExpressions)
        {
            if (expr.expressionName == painExpressionName)
            {
                hasPain = true;
                break;
            }
        }

        if (!hasPain)
        {
            Texture2D painTex = LoadTextureFromAssets("humanfigure3dmodel_basecolor_pain");
            if (painTex != null)
            {
                var newExpr = new FacialExpressionController.TextureExpression
                {
                    expressionName = painExpressionName,
                    expressionTexture = painTex
                };
                facialController.textureExpressions.Add(newExpr);
            }
        }
    }

    private Texture2D LoadTextureFromAssets(string name)
    {
        string[] searchPaths = new string[] {
            "Assets/Prefabs/Body/" + name + ".JPEG",
            "Assets/Textures/" + name + ".JPEG",
            "Assets/Prefabs/Body/" + name + ".jpg",
            "Assets/Textures/" + name + ".jpg"
        };
        #if UNITY_EDITOR
        foreach (var path in searchPaths)
        {
            var tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null) return tex;
        }
        #endif
        return null;
    }
}
