using UnityEngine;

/// <summary>
/// 自动定制 VR 射线与端点指示器（小红点）的视觉外观，使其明显且易用
/// 应该挂载在场景中的 UIHelpers 预制体上
/// </summary>
public class VRPointerVisualCustomizer : MonoBehaviour
{
    [Header("Line Settings (射线粗细与颜色)")]
    [Range(0.005f, 0.05f)] public float lineWidth = 0.015f; // 射线粗细 (默认一般是 0.005，改粗一点)
    public Color startColor = new Color(0f, 0.7f, 1f, 0.8f); // 起点颜色：医疗亮蓝
    public Color endColor = new Color(0f, 1f, 0.5f, 0.8f); // 终点颜色：正确亮绿

    [Header("End Dot Settings (端点指示器小球)")]
    [Range(0.01f, 0.1f)] public float dotScale = 0.04f; // 小球缩放尺寸 (默认极小，改为 0.04)
    public Color dotColor = new Color(0f, 1f, 0.5f, 1f); // 小球颜色：高亮绿色

    void Start()
    {
        // 1. 寻找并美化 LineRenderer (射线线条)
        LineRenderer line = GetComponentInChildren<LineRenderer>();
        if (line != null)
        {
            line.startWidth = lineWidth;
            line.endWidth = lineWidth * 0.4f;

            // 设置科技感渐变色
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(startColor, 0.0f), new GradientColorKey(endColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(startColor.a, 0.0f), new GradientAlphaKey(endColor.a, 1.0f) }
            );
            line.colorGradient = gradient;

            // 替换材质以支持发光
            line.material = new Material(Shader.Find("Sprites/Default"));
            Debug.Log("[VRPointerVisualCustomizer] LineRenderer customized successfully.");
        }

        // 2. 寻找并美化端点指示器小红点 (通常是名叫 Sphere 或 GazePointerRing 的物体)
        Transform sphere = null;
        
        // 递归寻找
        foreach (var renderer in GetComponentsInChildren<MeshRenderer>(true))
        {
            string name = renderer.name.ToLower();
            if (name.Contains("sphere") || name.Contains("pointer") || name.Contains("dot") || name.Contains("gaze"))
            {
                sphere = renderer.transform;
                break;
            }
        }

        if (sphere != null)
        {
            // 放大指示器尺寸
            sphere.localScale = new Vector3(dotScale, dotScale, dotScale);

            // 修改材质颜色为耀眼的高亮自发光绿色
            MeshRenderer renderer = sphere.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material customMat = null;
                // 探测当前项目的渲染管线着色器进行匹配，防止材质丢失变紫
                Shader hdrpShader = Shader.Find("HDRP/Lit");
                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                Shader standardShader = Shader.Find("Standard");

                if (hdrpShader != null) customMat = new Material(hdrpShader);
                else if (urpShader != null) customMat = new Material(urpShader);
                else customMat = new Material(standardShader != null ? standardShader : Shader.Find("Sprites/Default"));

                if (customMat != null)
                {
                    customMat.color = dotColor;
                    
                    // 开启材质自发光 (Emission)，在暗处极度显眼
                    customMat.EnableKeyword("_EMISSION");
                    // 适配不同材质的 Emissive 命名规范
                    if (hdrpShader != null)
                    {
                        customMat.SetColor("_EmissiveColor", dotColor * 3.0f);
                    }
                    else
                    {
                        customMat.SetColor("_EmissionColor", dotColor * 3.0f);
                    }
                    
                    renderer.material = customMat;
                }
            }
            Debug.Log($"[VRPointerVisualCustomizer] Dot indicator customized: Scale = {dotScale}, Color = Green.");
        }
        else
        {
            Debug.LogWarning("[VRPointerVisualCustomizer] End point Sphere/Dot indicator not found in hierarchy.");
        }
    }
}
