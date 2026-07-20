using UnityEngine;

/// <summary>
/// 动作与表情的运行时测试辅助脚本。
/// 挂载在角色上，运行游戏后通过键盘按键即可检验效果。
/// </summary>
public class AnimationTestHelper : MonoBehaviour
{
    private FacialExpressionController facialController;
    private CharacterActionController actionController;

    [Header("--- 测试配置 ---")]
    [Tooltip("测试自定义表情的名称 (需在 FacialExpressionController 的列表里注册过)")]
    public string testExpressionName = "Smile";

    [Tooltip("测试单次身体动作的 Trigger 参数名")]
    public string testActionTrigger = "PlayAction";

    void Start()
    {
        // 自动获取角色身上的控制器组件
        facialController = GetComponent<FacialExpressionController>();
        actionController = GetComponent<CharacterActionController>();

        if (facialController == null)
        {
            Debug.LogWarning("测试脚本找不到 FacialExpressionController，请确保它挂在同一个物体上！");
        }
        if (actionController == null)
        {
            Debug.LogWarning("测试脚本找不到 CharacterActionController，请确保它挂在同一个物体上！");
        }
    }

    void Update()
    {
        // ================= 面部表情检验 =================
        
        // 1. 自动眨眼检验：
        // 只要你运行了游戏，且在 FacialExpressionController 上勾选了 Auto Blink，
        // NPC 就会每隔 2~5 秒自动闭眼眨一下。无需按下任何键，直接观察角色即可。

        // 2. 键盘数字键 1 触发自定义表情 (例如 Smile)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (facialController != null)
            {
                facialController.SetTextureExpression(testExpressionName, pauseBlink: true);
                Debug.Log($"[测试] 已触发自定义表情：{testExpressionName}，并暂停自动眨眼。");
            }
        }

        // 3. 键盘数字键 2 恢复默认睁眼状态 (并重新启用自动眨眼)
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (facialController != null)
            {
                facialController.ResetToDefaultTexture();
                Debug.Log("[测试] 已恢复默认表情，重新开启自动眨眼。");
            }
        }


        // ================= 身体动作检验 =================

        // 4. 键盘空格键 (Space) 触发一次性动作
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (actionController != null)
            {
                actionController.TriggerActionByName(testActionTrigger);
                Debug.Log($"[测试] 已发送 Trigger 参数: '{testActionTrigger}' 以触发肢体动作。");
            }
        }
    }
}
