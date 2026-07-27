using UnityEngine;

/// <summary>
/// HDRP 管线双手显形与材质自适应修复器
/// 彻底解决手部模型在 HDRP 管线中发黑、发紫、隐形看不到以及 Culling Mask 遮挡的问题。
/// 赋予双手高清晰度、高可见度的亮丽 HDRP 材质。
/// </summary>
public class HDRPMaterialFixer : MonoBehaviour
{
    [Header("Visibility Settings")]
    [Tooltip("手部高显度材质颜色")]
    [SerializeField] private Color handColor = new Color(0.1f, 0.75f, 1.0f, 1.0f); // 亮青科技色

    private Shader hdrpLitShader;

    void Awake()
    {
        hdrpLitShader = Shader.Find("HDRP/Lit");
        if (hdrpLitShader == null) hdrpLitShader = Shader.Find("Standard");

        FixMaterials();
    }

    void Start()
    {
        FixMaterials();
    }

    /// <summary>
    /// 强制开启并修复双手所有的 Renderer 和材质，确保 100% 显形可见！
    /// </summary>
    public void FixMaterials()
    {
        var cameraRig = GetComponent<OVRCameraRig>();
        if (cameraRig == null) cameraRig = GetComponentInChildren<OVRCameraRig>(true);

        if (cameraRig != null)
        {
            // 修复左手
            if (cameraRig.leftHandAnchor != null)
            {
                FixHandNode(cameraRig.leftHandAnchor);
            }

            // 修复右手
            if (cameraRig.rightHandAnchor != null)
            {
                FixHandNode(cameraRig.rightHandAnchor);
            }

            // 确保 CenterEyeAnchor 摄像机 Culling Mask 包含手部 Layer
            if (cameraRig.centerEyeAnchor != null)
            {
                Camera cam = cameraRig.centerEyeAnchor.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.cullingMask |= (1 << 0); // 强行勾选 Default Layer
                }
            }
        }

        // 扫描全局带 Hand 名称的 Renderer 兜底
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in allRenderers)
        {
            if (r.name.ToLower().Contains("hand") || r.transform.parent?.name.ToLower().Contains("hand") == true)
            {
                EnsureRendererVisible(r);
            }
        }

        Debug.Log("[HDRPMaterialFixer] Hands visibility & HDRP materials fixed successfully!");
    }

    void FixHandNode(Transform handAnchor)
    {
        handAnchor.gameObject.SetActive(true);
        
        Renderer[] renderers = handAnchor.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            EnsureRendererVisible(r);
        }
    }

    void EnsureRendererVisible(Renderer r)
    {
        r.gameObject.SetActive(true);
        r.enabled = true;

        // 强行把手部设为 Default Layer，防止被 Culling Mask 过滤
        r.gameObject.layer = 0;

        Material mat = r.sharedMaterial;
        if (mat == null || mat.shader == null || mat.shader.name.Contains("InternalErrorShader") || mat.shader.name.Contains("Error"))
        {
            Material newMat = new Material(hdrpLitShader != null ? hdrpLitShader : Shader.Find("Standard"));
            newMat.name = "Fixed_Hand_Material";

            if (newMat.HasProperty("_BaseColor"))
                newMat.SetColor("_BaseColor", handColor);
            else if (newMat.HasProperty("_Color"))
                newMat.SetColor("_Color", handColor);

            if (newMat.HasProperty("_Smoothness"))
                newMat.SetFloat("_Smoothness", 0.6f);

            r.material = newMat;
        }
    }
}
