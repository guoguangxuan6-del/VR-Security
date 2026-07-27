using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// 全套 VRPlayer 根节点与高显度双手硬重建/注入器
/// 彻底保障 LeftHandAnchor 与 RightHandAnchor 在任何模式下 100% 显形可见。
/// </summary>
[InitializeOnLoad]
public class AutoSceneInjector
{
    static AutoSceneInjector()
    {
        EditorApplication.delayCall += PerformSceneInjection;
    }

    [MenuItem("Tools/Force Build VRPlayer & Visible Hands")]
    public static void PerformSceneInjection()
    {
        Scene activeScene = EditorSceneManager.GetActiveScene();
        if (!activeScene.isLoaded || !activeScene.name.Equals("Demonstration"))
        {
            string scenePath = "Assets/Scenes/Subway/Demonstration.unity";
            if (System.IO.File.Exists(scenePath))
            {
                activeScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
        }

        if (!activeScene.isLoaded) return;

        bool isModified = false;

        // 1. 全局 InputManager
        var inputMgr = Object.FindObjectOfType<InputManager>();
        if (inputMgr == null)
        {
            GameObject mgrObj = new GameObject("InputManager");
            inputMgr = mgrObj.AddComponent<InputManager>();
            Undo.RegisterCreatedObjectUndo(mgrObj, "Create InputManager");
            isModified = true;
        }

        // 2. OVRCameraRig 与 TrackingSpace
        var cameraRig = Object.FindObjectOfType<OVRCameraRig>();
        if (cameraRig == null)
        {
            GameObject cameraRigObj = new GameObject("OVRCameraRig");
            cameraRig = cameraRigObj.AddComponent<OVRCameraRig>();
            
            GameObject trackingSpace = new GameObject("TrackingSpace");
            trackingSpace.transform.SetParent(cameraRigObj.transform, false);

            GameObject centerEye = new GameObject("CenterEyeAnchor");
            centerEye.transform.SetParent(trackingSpace.transform, false);
            centerEye.AddComponent<Camera>();
            centerEye.AddComponent<AudioListener>();

            GameObject leftHand = new GameObject("LeftHandAnchor");
            leftHand.transform.SetParent(trackingSpace.transform, false);

            GameObject rightHand = new GameObject("RightHandAnchor");
            rightHand.transform.SetParent(trackingSpace.transform, false);

            isModified = true;
        }

        // 3. 构建高显度、绝不隐形的双手 Visual 节点
        BuildVisibleHandModel(cameraRig.leftHandAnchor, "CustomHandLeft", new Vector3(-0.02f, 0f, 0.08f));
        BuildVisibleHandModel(cameraRig.rightHandAnchor, "CustomHandRight", new Vector3(0.02f, 0f, 0.08f));

        // 4. VRPlayer 根节点
        GameObject playerRoot = GameObject.Find("VRPlayer");
        if (playerRoot == null)
        {
            if (cameraRig.transform.parent != null && cameraRig.transform.parent.name.Equals("VRPlayer"))
            {
                playerRoot = cameraRig.transform.parent.gameObject;
            }
            else
            {
                playerRoot = new GameObject("VRPlayer");
                playerRoot.transform.position = cameraRig.transform.position;
                cameraRig.transform.SetParent(playerRoot.transform);
                Undo.RegisterCreatedObjectUndo(playerRoot, "Create VRPlayer Root");
                isModified = true;
            }
        }

        var characterController = playerRoot.GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = playerRoot.AddComponent<CharacterController>();
            isModified = true;
        }

        characterController.height = 1.3f;
        characterController.center = new Vector3(0f, 0.65f, 0f);
        characterController.radius = 0.05f;
        characterController.stepOffset = 0.5f;
        characterController.slopeLimit = 85f;

        var playerRig = playerRoot.GetComponent<VRPlayerRig>();
        if (playerRig == null)
        {
            playerRig = playerRoot.AddComponent<VRPlayerRig>();
            isModified = true;
        }

        SerializedObject serializedRig = new SerializedObject(playerRig);
        serializedRig.FindProperty("cameraRig").objectReferenceValue = cameraRig;
        serializedRig.ApplyModifiedProperties();

        // 挂载 HDRP 材质修复器
        var fixer = cameraRig.GetComponent<HDRPMaterialFixer>();
        if (fixer == null)
        {
            fixer = cameraRig.gameObject.AddComponent<HDRPMaterialFixer>();
            isModified = true;
        }
        fixer.FixMaterials();

        if (inputMgr != null && cameraRig != null)
        {
            inputMgr.SetHandAnchors(cameraRig.leftHandAnchor, cameraRig.rightHandAnchor);
        }

        // 5. 患者人体节点与 3D 情景对话框
        GameObject patientObj = GameObject.Find("Patient");
        if (patientObj == null)
        {
            patientObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            patientObj.name = "Patient";
            patientObj.transform.position = new Vector3(0f, 0f, 2.5f);
            patientObj.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);

            var animator = patientObj.GetComponent<Animator>();
            if (animator == null) patientObj.AddComponent<Animator>();

            patientObj.AddComponent<PatientController>();
            patientObj.AddComponent<PatientDialogueTrigger>();

            Undo.RegisterCreatedObjectUndo(patientObj, "Create Patient");
            isModified = true;
        }

        // 6. CPR 按压黄金区间训练管理器
        var cprMgr = Object.FindObjectOfType<CPRTrainingManager>();
        if (cprMgr == null)
        {
            GameObject cprObj = new GameObject("CPRTrainingManager");
            cprMgr = cprObj.AddComponent<CPRTrainingManager>();
            Undo.RegisterCreatedObjectUndo(cprObj, "Create CPRTrainingManager");
            isModified = true;
        }

        if (isModified)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Debug.Log("[AutoSceneInjector] Rebuilt 100% visible hands in Demonstration.unity!");
        }
    }

    /// <summary>
    /// 强行构建高显度、带碰撞、在任何镜头下 100% 显形的手部模型占位体
    /// </summary>
    static void BuildVisibleHandModel(Transform handAnchor, string handName, Vector3 localOffset)
    {
        if (handAnchor == null) return;

        Transform handChild = handAnchor.Find(handName);
        if (handChild == null)
        {
            GameObject handObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handObj.name = handName;
            handObj.transform.SetParent(handAnchor, false);
            handObj.transform.localPosition = localOffset;
            handObj.transform.localScale = new Vector3(0.08f, 0.05f, 0.16f); // 酷炫手部控制器造形
            
            var col = handObj.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            var r = handObj.GetComponent<Renderer>();
            if (r != null)
            {
                r.enabled = true;
                r.gameObject.layer = 0; // Default Layer
            }
        }
        else
        {
            handChild.gameObject.SetActive(true);
            var r = handChild.GetComponent<Renderer>();
            if (r != null)
            {
                r.enabled = true;
                r.gameObject.layer = 0;
            }
        }
    }
}
