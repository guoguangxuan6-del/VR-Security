using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// VR-Security NPC 一键配置与修复工具。
/// </summary>
public class NPCSimulatorSetupTool : EditorWindow
{
    [MenuItem("VR-Security/NPC 一键配置与修复")]
    public static void ExecuteOneClickSetup()
    {
        Debug.Log("================ 开始执行 NPC 一键配置与修复 ================");
        
        GameObject npcGO = FindNPCInScene();
        if (npcGO == null)
        {
            EditorUtility.DisplayDialog("提示", "未能在场景中找到名为 'tripo_node_e381317b' 的 NPC 物体。\n请确认你打开了正确的场景，或者当前场景中确实包含该 NPC 模型。", "确定");
            return;
        }

        if (!npcGO.activeSelf)
        {
            npcGO.SetActive(true);
        }

        Transform parent = npcGO.transform.parent;
        while (parent != null)
        {
            if (!parent.gameObject.activeSelf)
            {
                parent.gameObject.SetActive(true);
            }
            parent = parent.parent;
        }

        // 核心修正：挂载控制脚本时，我们应该直接挂载在包含真正的骨骼 Animator 的【父级物体/根物体】上！
        // 这样动作脚本才能控制整个人的运动，并且材质脚本也会自动向下索引找到 tripo 的网格渲染器。
        GameObject rootNPC = npcGO.transform.parent != null ? npcGO.transform.parent.gameObject : npcGO;
        
        FacialExpressionController facial = GetOrAddComponent<FacialExpressionController>(rootNPC);
        CharacterActionController action = GetOrAddComponent<CharacterActionController>(rootNPC);
        NPCInjuryEventController injury = GetOrAddComponent<NPCInjuryEventController>(rootNPC);

        // 确保子级的网格节点上也别有重复残留的脚本，防止多重调用冲突
        RemoveComponentIfExist<FacialExpressionController>(npcGO);
        RemoveComponentIfExist<CharacterActionController>(npcGO);
        RemoveComponentIfExist<NPCInjuryEventController>(npcGO);

        // 自动配置表情控制器 (它会自动从子节点搜索 tripo 的 Renderer 材质)
        facial.useTextureMode = true;
        facial.autoBlink = true;
        facial.targetRenderer = npcGO.GetComponent<Renderer>();
        
        facial.defaultTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Prefabs/Body/humanfigure3dmodel_basecolor.JPEG");
        facial.blinkTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Prefabs/Body/humanfigure3dmodel_basecolor_blink.JPEG");

        if (facial.defaultTexture == null)
            facial.defaultTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/humanfigure3dmodel_basecolor.JPEG");
        if (facial.blinkTexture == null)
            facial.blinkTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/humanfigure3dmodel_basecolor_blink.JPEG");

        // 配置事件控制器参数
        SetupInjuryController(injury);

        // 为真正的根节点 Animator 绑定并连线 Animator Controller
        SetupAnimatorController(rootNPC);

        // 自动修复场景地表的物理碰撞器
        int fixedGrounds = FixGroundColliders();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        string reportMsg = $"一键动作与动画控制器修复配置成功！\n\n" +
                          $"1. 成功为 NPC 根节点 '{rootNPC.name}' 关联并生成了控制动画的 NPC_Controller！\n" +
                          $"2. 自动拉好了 'PainCollapse' 与 'StandUp' 动画连线！\n" +
                          $"3. 自动将控制脚本挂载到根节点上并完成了表情贴图关联！\n" +
                          $"4. 自动检测并修复了场景中 {fixedGrounds} 个地面的 Collider 碰撞体\n\n" +
                          $"请现在重新点击 Unity 顶部的 Play 运行游戏，测试 K 键倒地与 R 键起身！";

        EditorUtility.DisplayDialog("一键动画配置成功", reportMsg, "完成");
    }

    private static GameObject FindNPCInScene()
    {
        GameObject[] allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in allGOs)
        {
            if (go.name == "tripo_node_e381317b" && !EditorUtility.IsPersistent(go))
            {
                return go;
            }
        }
        return null;
    }

    private static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null)
        {
            comp = go.AddComponent<T>();
            Debug.Log($"[一键配置] ★自动为 NPC 根物体 '{go.name}' 挂载了组件: {typeof(T).Name}");
        }
        return comp;
    }

    private static void RemoveComponentIfExist<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp != null)
        {
            DestroyImmediate(comp);
            Debug.Log($"[一键配置] 已清理子网格节点上重复的冗余组件: {typeof(T).Name}");
        }
    }

    private static void SetupInjuryController(NPCInjuryEventController injury)
    {
        injury.enableKeyboardDebug = true;
        injury.painExpressionName = "Pain";
        injury.collapseTriggerName = "PainCollapse";
        injury.injuredBoolName = "IsInjured";
    }

    private static void SetupAnimatorController(GameObject rootNPC)
    {
        Animator animator = rootNPC.GetComponent<Animator>();
        if (animator == null) animator = rootNPC.AddComponent<Animator>();

        string animFolder = "Assets/Animations";
        if (!AssetDatabase.IsValidFolder(animFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Animations");
        }

        string controllerPath = animFolder + "/NPC_Controller.controller";
        
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            Debug.Log("[一键配置] 成功在 'Assets/Animations/' 下创建了全新的 Animator Controller。");
        }

        var rootStateMachine = controller.layers[0].stateMachine;

        // 加载刚才生成的两个 .anim 动画片段
        AnimationClip collapseClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/NPC_PainCollapse.anim");
        AnimationClip standClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/NPC_StandUp.anim");

        if (collapseClip == null || standClip == null)
        {
            Debug.LogError("[一键配置] 错误：未能在 Assets 中找到 NPC_PainCollapse 或 NPC_StandUp 动画！请确认文件未被删除。");
            return;
        }

        // 创建状态
        AnimatorState idleState = FindOrCreateState(rootStateMachine, "Idle", null);
        AnimatorState collapseState = FindOrCreateState(rootStateMachine, "PainCollapse", collapseClip);
        AnimatorState standState = FindOrCreateState(rootStateMachine, "StandUp", standClip);

        rootStateMachine.defaultState = idleState;

        // 创建 Trigger 参数并建立线
        AddTransitionWithTrigger(idleState, collapseState, "PainCollapse", controller);
        AddTransitionWithTrigger(collapseState, standState, "StandUp", controller);
        AddExitTransition(standState, idleState);

        // 连带配置 IsInjured 的 Bool 参数
        bool hasInjuredBool = false;
        foreach (var param in controller.parameters)
        {
            if (param.name == "IsInjured") { hasInjuredBool = true; break; }
        }
        if (!hasInjuredBool)
        {
            controller.AddParameter("IsInjured", AnimatorControllerParameterType.Bool);
        }

        animator.runtimeAnimatorController = controller;
        Debug.Log($"[一键配置] 成功将控制器绑定至真正的 NPC 根物体 Animator: '{rootNPC.name}'。");
    }

    private static AnimatorState FindOrCreateState(AnimatorStateMachine stateMachine, string stateName, Motion motion)
    {
        foreach (var state in stateMachine.states)
        {
            if (state.state.name == stateName)
            {
                if (motion != null) state.state.motion = motion;
                return state.state;
            }
        }
        var newState = stateMachine.AddState(stateName);
        newState.motion = motion;
        return newState;
    }

    private static void AddTransitionWithTrigger(AnimatorState source, AnimatorState destination, string triggerName, AnimatorController controller)
    {
        bool hasParam = false;
        foreach (var param in controller.parameters)
        {
            if (param.name == triggerName) { hasParam = true; break; }
        }
        if (!hasParam)
        {
            controller.AddParameter(triggerName, AnimatorControllerParameterType.Trigger);
        }

        foreach (var transition in source.transitions)
        {
            if (transition.destinationState == destination) return;
        }

        var newTransition = source.AddTransition(destination);
        newTransition.AddCondition(AnimatorConditionMode.If, 0, triggerName);
        newTransition.hasExitTime = false;
    }

    private static void AddExitTransition(AnimatorState source, AnimatorState destination)
    {
        foreach (var transition in source.transitions)
        {
            if (transition.destinationState == destination) return;
        }
        var newTransition = source.AddTransition(destination);
        newTransition.hasExitTime = true;
        newTransition.exitTime = 0.95f; 
    }

    private static int FixGroundColliders()
    {
        int fixedCount = 0;
        MeshRenderer[] renderers = Object.FindObjectsOfType<MeshRenderer>();
        
        foreach (var r in renderers)
        {
            string lowerName = r.gameObject.name.ToLower();
            if (lowerName.Contains("floor") || lowerName.Contains("ground") || lowerName.Contains("subway") || lowerName.Contains("platform"))
            {
                if (r.gameObject.GetComponent<Collider>() == null)
                {
                    r.gameObject.AddComponent<MeshCollider>();
                    fixedCount++;
                    Debug.Log($"[一键配置] ★已自动修复场景地面碰撞体：为 '{r.gameObject.name}' 挂载了 MeshCollider");
                }
            }
        }
        return fixedCount;
    }
}
