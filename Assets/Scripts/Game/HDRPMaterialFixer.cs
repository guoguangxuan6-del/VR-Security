using UnityEngine;

/// <summary>
/// HDRP 材质自适应修复工具
/// 自动扫描子物体下的所有 Renderer，将不兼容的 Built-in 材质（如 Standard 等导致的粉红色/隐形）替换为 HDRP 材质。
/// 挂载在 OVRCameraRig、手部模型或 UIHelpers 等 VR 硬件物体上。
/// </summary>
public class HDRPMaterialFixer : MonoBehaviour
{
    void Awake()
    {
        FixMaterials();
    }

    public void FixMaterials()
    {
        Shader hdrpLitShader = Shader.Find("HDRP/Lit");
        Shader hdrpUnlitShader = Shader.Find("HDRP/Unlit");

        if (hdrpLitShader == null && hdrpUnlitShader == null)
        {
            Debug.LogWarning("[HDRPMaterialFixer] HDRP shaders not found in project. Skipping fix.");
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        int fixedCount = 0;

        foreach (Renderer r in renderers)
        {
            Material[] mats = r.materials;
            bool changed = false;

            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;

                Shader currentShader = mats[i].shader;
                string shaderName = currentShader != null ? currentShader.name : "Null";

                // 判断是否为不兼容的 Built-in/Legacy 着色器
                if (shaderName == "Standard" || 
                    shaderName.Contains("Legacy Shaders") || 
                    shaderName.Contains("Mobile/") ||
                    shaderName == "Particles/Additive" ||
                    shaderName == "Sprites/Default" && !(r is SpriteRenderer))
                {
                    // 挑选适当的 HDRP 替代着色器
                    Shader targetShader = hdrpLitShader;
                    
                    // 如果是激光线、射线、指示小球、UI，使用 Unlit 无光照材质，防止在暗处发黑
                    if (r is LineRenderer || 
                        r.name.Contains("Laser") || 
                        r.name.Contains("Pointer") || 
                        r.name.Contains("Ring") || 
                        r.name.Contains("Dot") || 
                        r.name.Contains("Sphere"))
                    {
                        targetShader = hdrpUnlitShader != null ? hdrpUnlitShader : hdrpLitShader;
                    }

                    if (targetShader != null)
                    {
                        // 拷贝原先的主色和主贴图
                        Color origColor = mats[i].HasProperty("_Color") ? mats[i].color : Color.white;
                        Texture origTex = mats[i].HasProperty("_MainTex") ? mats[i].mainTexture : null;

                        mats[i].shader = targetShader;

                        // 适配 HDRP 材质属性名
                        if (mats[i].HasProperty("_BaseColor"))
                            mats[i].SetColor("_BaseColor", origColor);
                        if (mats[i].HasProperty("_BaseColorMap") && origTex != null)
                            mats[i].SetTexture("_BaseColorMap", origTex);

                        // 激光和瞄准小球自发光处理
                        if (targetShader == hdrpUnlitShader && mats[i].HasProperty("_EmissiveColor"))
                        {
                            mats[i].SetColor("_EmissiveColor", origColor * 2.0f);
                        }

                        changed = true;
                        fixedCount++;
                    }
                }
            }

            if (changed)
            {
                r.materials = mats; // 重新赋值
            }
        }

        Debug.Log($"[HDRPMaterialFixer] Successfully scanned and fixed {fixedCount} materials on {gameObject.name} for HDRP compatibility.");
    }
}
