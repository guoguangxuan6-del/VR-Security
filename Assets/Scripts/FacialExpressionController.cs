using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 终极免材质提取表情控制器（基于 MaterialPropertyBlock 材质属性块）。
/// 无需提取 FBX 嵌入材质，直接在 GPU 渲染层面强行覆盖贴图，解决任何只读材质无法在运行时修改的问题。
/// </summary>
public class FacialExpressionController : MonoBehaviour
{
    [Header("--- 模式选择 ---")]
    [Tooltip("是否使用贴图切换模式")]
    public bool useTextureMode = true;

    [Header("--- 渲染器配置 (留空将自动绑定当前物体) ---")]
    [Tooltip("角色网格渲染器，留空则自动获取当前物体或子物体的 Renderer")]
    public Renderer targetRenderer;

    [Header("--- 贴图配置 (留空将自动在 Assets 中搜寻) ---")]
    public Texture2D defaultTexture;
    public Texture2D blinkTexture;
    
    [Tooltip("贴图属性名称。留空会自动检测是 _BaseColorMap、_BaseMap 还是 _MainTex")]
    public string texturePropertyName = "";

    [System.Serializable]
    public struct TextureExpression
    {
        public string expressionName;
        public Texture2D expressionTexture;
    }
    public List<TextureExpression> textureExpressions = new List<TextureExpression>();

    [Header("--- 自动眨眼配置 ---")]
    public bool autoBlink = true;
    public float minBlinkInterval = 2.0f;
    public float maxBlinkInterval = 5.0f;
    public float blinkDuration = 0.15f;

    private Coroutine blinkCoroutine;
    private bool isCustomExpressionActive = false;
    
    // Unity 材质属性块：允许直接修改 GPU 渲染属性，而无需实例化只读材质
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        if (useTextureMode)
        {
            InitializeController();
        }
    }

    private void InitializeController()
    {
        // 1. 自动绑定 Renderer
        if (targetRenderer == null)
        {
            // 优先找 tripo 命名的渲染器
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r.gameObject.name.Contains("tripo") || r.gameObject.name.Contains("Body"))
                {
                    targetRenderer = r;
                    break;
                }
            }

            // 兜底：获取第一个子渲染器
            if (targetRenderer == null && renderers.Length > 0)
            {
                targetRenderer = renderers[0];
            }
        }

        if (targetRenderer == null)
        {
            Debug.LogError("[表情控制器] ❌错误：未能在当前物体及其子级找到任何 Renderer！请在 Inspector 中拖入 Target Renderer。");
            return;
        }

        Debug.Log($"[表情控制器] 🚀成功绑定渲染器: '{targetRenderer.gameObject.name}'");

        // 2. 自动检测贴图属性名 (基于当前材质)
        Material sharedMat = targetRenderer.sharedMaterial;
        if (sharedMat == null)
        {
            Debug.LogError("[表情控制器] ❌错误：Renderer 上没有材质！");
            return;
        }

        if (string.IsNullOrEmpty(texturePropertyName))
        {
            if (sharedMat.HasProperty("_BaseColorMap")) texturePropertyName = "_BaseColorMap";
            else if (sharedMat.HasProperty("_BaseMap")) texturePropertyName = "_BaseMap";
            else texturePropertyName = "_MainTex";
            Debug.Log($"[表情控制器] 自动匹配贴图属性名称为: {texturePropertyName}");
        }

        // 3. 自动匹配和加载贴图
        if (defaultTexture == null)
        {
            defaultTexture = AutoLoadTexture("humanfigure3dmodel_basecolor");
        }
        if (blinkTexture == null)
        {
            blinkTexture = AutoLoadTexture("humanfigure3dmodel_basecolor_blink");
        }

        if (defaultTexture == null || blinkTexture == null)
        {
            Debug.LogError("[表情控制器] ❌错误：无法自动加载睁眼/闭眼贴图，请检查图片是否在项目内。");
            return;
        }

        // 4. 初始化 MaterialPropertyBlock
        propBlock = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(propBlock);

        // 应用睁眼贴图
        propBlock.SetTexture(texturePropertyName, defaultTexture);
        targetRenderer.SetPropertyBlock(propBlock);

        // 5. 启动自动眨眼
        if (autoBlink)
        {
            StartAutoBlink();
        }
    }

    private Texture2D AutoLoadTexture(string name)
    {
        string[] searchPaths = new string[] {
            "Assets/Prefabs/Body/" + name + ".JPEG",
            "Assets/Prefabs/Body/tripo_convert_e381317b-0f4a-46ef-9531-9449932a8212.fbm/" + name + ".JPEG",
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

    public void StartAutoBlink()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        if (targetRenderer != null && defaultTexture != null && blinkTexture != null)
        {
            blinkCoroutine = StartCoroutine(AutoBlinkRoutine());
        }
    }

    public void StopAutoBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
    }

    private IEnumerator AutoBlinkRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minBlinkInterval, maxBlinkInterval);
            yield return new WaitForSeconds(waitTime);

            if (!isCustomExpressionActive && propBlock != null && targetRenderer != null)
            {
                // 强制闭眼贴图
                propBlock.SetTexture(texturePropertyName, blinkTexture);
                targetRenderer.SetPropertyBlock(propBlock);

                yield return new WaitForSeconds(blinkDuration);

                // 强制还原睁眼贴图
                propBlock.SetTexture(texturePropertyName, defaultTexture);
                targetRenderer.SetPropertyBlock(propBlock);
            }
        }
    }

    public void SetTextureExpression(string expressionName, bool pauseBlink = true)
    {
        if (!useTextureMode || targetRenderer == null || propBlock == null) return;

        string lowerName = expressionName.ToLower();

        if (lowerName == "default" || lowerName == "normal" || lowerName == "open")
        {
            ResetToDefaultTexture();
            return;
        }
        if (lowerName == "blink" || lowerName == "close")
        {
            if (blinkTexture != null)
            {
                isCustomExpressionActive = pauseBlink;
                propBlock.SetTexture(texturePropertyName, blinkTexture);
                targetRenderer.SetPropertyBlock(propBlock);
            }
            return;
        }

        TextureExpression expr = textureExpressions.Find(t => t.expressionName == expressionName);
        if (expr.expressionTexture == null)
        {
            // Smile 容错为内置闭眼效果
            if (lowerName == "smile" && blinkTexture != null)
            {
                isCustomExpressionActive = pauseBlink;
                propBlock.SetTexture(texturePropertyName, blinkTexture);
                targetRenderer.SetPropertyBlock(propBlock);
            }
            return;
        }

        isCustomExpressionActive = pauseBlink;
        propBlock.SetTexture(texturePropertyName, expr.expressionTexture);
        targetRenderer.SetPropertyBlock(propBlock);
    }

    public void ResetToDefaultTexture()
    {
        if (targetRenderer != null && propBlock != null && defaultTexture != null)
        {
            propBlock.SetTexture(texturePropertyName, defaultTexture);
            targetRenderer.SetPropertyBlock(propBlock);
        }
        isCustomExpressionActive = false;
    }
}
