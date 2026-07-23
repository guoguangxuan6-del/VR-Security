using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// VR 虚拟键盘助手 - 挂载在 Canvas (如 LoginCanvas) 上。
/// 游戏启动时会自动寻找子节点中的 InputField 并为其装配一个 3D 射线点击的虚拟键盘，解决 VR 环境的文字输入问题。
/// </summary>
public class VRVirtualKeyboardHelper : MonoBehaviour
{
    private Component activeInputField; // 兼容原生 InputField 和 TMP_InputField
    private GameObject keyboardPanel;

    // 键盘键位定义 (包含数字、英文字母及控制键)
    private readonly string[] keys = {
        "1", "2", "3", "4", "5", "6", "7", "8", "9", "0",
        "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P",
        "A", "S", "D", "F", "G", "H", "J", "K", "L", "Backspace",
        "Z", "X", "C", "V", "B", "N", "M", "Clear", "Close", "Enter"
    };

    void Start()
    {
        // VR 虚拟键盘仅在 VR 模式下启用（桌面端键鼠不需要此组件）
        // feature/no-vr 分支不会加载 XR 包，此处二次保险避免误挂载后干扰桌面端输入
        #if !ENABLE_VR
        this.enabled = false;
        return;
        #endif

        // 1. 查找并绑定场景内所有传统的 InputField
        InputField[] legacyInputs = GetComponentsInChildren<InputField>(true);
        foreach (var input in legacyInputs)
        {
            BindSelectEvent(input.gameObject, () => { OnInputFieldSelect(input); });
        }

        // 2. 查找并绑定场景内所有 TextMeshPro 的 TMP_InputField
        TMP_InputField[] tmpInputs = GetComponentsInChildren<TMP_InputField>(true);
        foreach (var input in tmpInputs)
        {
            BindSelectEvent(input.gameObject, () => { OnInputFieldSelect(input); });
        }

        // 3. 动态绘制虚拟键盘 UI
        CreateKeyboardUI();
    }

    // 监听输入框的选择 Focus 动作
    void BindSelectEvent(GameObject target, System.Action onSelectAction)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null) trigger = target.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Select;
        entry.callback.AddListener((data) => { onSelectAction?.Invoke(); });
        trigger.triggers.Add(entry);
    }

    void OnInputFieldSelect(Component inputField)
    {
        activeInputField = inputField;
        if (keyboardPanel != null)
        {
            keyboardPanel.SetActive(true);
            // 自动将键盘贴靠在当前 Canvas 的偏右下侧位置，避免遮挡
            keyboardPanel.transform.localPosition = new Vector3(280f, -80f, 0f);
            keyboardPanel.transform.localScale = Vector3.one;
        }
    }

    void CreateKeyboardUI()
    {
        // 创建键盘的空物体
        keyboardPanel = new GameObject("VR_VirtualKeyboard");
        keyboardPanel.transform.SetParent(this.transform, false);
        keyboardPanel.SetActive(false); // 默认隐藏，选中输入框时浮现

        // 挂载 Image 作为键盘半透明科幻蓝色背景
        Image bgImage = keyboardPanel.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.05f, 0.15f, 0.92f);

        RectTransform panelRect = keyboardPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(400f, 250f);

        // 使用 Grid Layout Group 自动网格布局按键
        GridLayoutGroup grid = keyboardPanel.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(35f, 35f);
        grid.spacing = new Vector2(4f, 4f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 10; // 每排 10 个按键
        grid.padding = new RectOffset(8, 8, 8, 8);
        grid.childAlignment = TextAnchor.MiddleCenter;

        // 生成键盘按键
        foreach (string key in keys)
        {
            GameObject btnObj = new GameObject("Key_" + key);
            btnObj.transform.SetParent(keyboardPanel.transform, false);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.15f, 0.15f, 0.25f, 1f);

            Button btn = btnObj.AddComponent<Button>();
            
            // 设置射线移入的高亮过渡颜色 (亮蓝色)
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.15f, 0.15f, 0.25f, 1f);
            cb.highlightedColor = new Color(0f, 0.7f, 1f, 1f); // 指上去变亮蓝
            cb.pressedColor = new Color(0f, 0.5f, 0.8f, 1f); // 点击变暗蓝
            btn.colors = cb;

            // 渲染按键文本
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            TextMeshProUGUI txtTMP = textObj.AddComponent<TextMeshProUGUI>();
            txtTMP.text = key;
            txtTMP.fontSize = 8f;
            txtTMP.alignment = TextAlignmentOptions.Center;
            txtTMP.color = Color.white;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            string localKey = key;
            btn.onClick.AddListener(() => { OnKeyClick(localKey); });
        }
    }

    void OnKeyClick(string key)
    {
        if (activeInputField == null) return;

        string currentText = GetInputFieldText();

        if (key == "Backspace")
        {
            if (currentText.Length > 0)
                SetInputFieldText(currentText.Substring(0, currentText.Length - 1));
        }
        else if (key == "Clear")
        {
            SetInputFieldText("");
        }
        else if (key == "Close")
        {
            keyboardPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
        }
        else if (key == "Enter")
        {
            keyboardPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            TriggerInputFieldSubmit();
        }
        else
        {
            // 添加字符
            SetInputFieldText(currentText + key);
        }

        // 重新聚焦输入框
        ReactivateInputField();
    }

    string GetInputFieldText()
    {
        if (activeInputField is TMP_InputField tmp) return tmp.text;
        if (activeInputField is InputField legacy) return legacy.text;
        return "";
    }

    void SetInputFieldText(string text)
    {
        if (activeInputField is TMP_InputField tmp) tmp.text = text;
        if (activeInputField is InputField legacy) legacy.text = text;
    }

    void TriggerInputFieldSubmit()
    {
        if (activeInputField is TMP_InputField tmp) tmp.onSubmit?.Invoke(tmp.text);
        if (activeInputField is InputField legacy) legacy.onSubmit?.Invoke(legacy.text);
    }

    void ReactivateInputField()
    {
        if (activeInputField is TMP_InputField tmp) tmp.ActivateInputField();
        if (activeInputField is InputField legacy) legacy.ActivateInputField();
    }
}
