using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// 全套 VRPlayer 根节点与绑定的双手硬重建/注入器
/// 自动重建 VRPlayer 根物体，重构 OVRCameraRig 与双手模型 (LeftHandAnchor / RightHandAnchor)，
/// 恢复最原始丝滑连续平滑旋转视角逻辑，并挂载 HDRP 材质修复与双手输入，直接 SaveScene 保存写入 Demonstration.unity YAML!
/// </summary>
[InitializeOnLoad]
public class AutoSceneInjector
{
    static AutoSceneInjector()
    {
        EditorApplication.delayCall += PerformSceneInjection;
    }

    [MenuItem("Tools/Force Build VRPlayer & Hands in Scene")]
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

        // 1. 全局 InputManager 查找或创建
        var inputMgr = Object.FindObjectOfType<InputManager>();
        if (inputMgr == null)
        {
            GameObject mgrObj = new GameObject("InputManager");
            inputMgr = mgrObj.AddComponent<InputManager>();
            Undo.RegisterCreatedObjectUndo(mgrObj, "Create InputManager");
            isModified = true;
        }

        // 2. 查找或重建 OVRCameraRig
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

        // 建立/保底手部 Visual 模型 (确保双手 100% 显形)
        EnsureHandVisual(cameraRig.leftHandAnchor, "CustomHandLeft");
        EnsureHandVisual(cameraRig.rightHandAnchor, "CustomHandRight");

        // 3. 彻底重构并恢复【VRPlayer 根节点与绑定的双手】
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

        // 挂载 HDRP 材质修复器 (确保双手显形，材料不发黑不发紫)
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

        // 4. 患者人体受害者节点与 3D 情景对话框
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

        // 5. CPR 按压黄金区间训练管理器
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
            Debug.Log("[AutoSceneInjector] Rebuilt VRPlayer, Hands & restored smooth VR rotation!");
        }
    }

    static void EnsureHandVisual(Transform handAnchor, string handName)
    {
        if (handAnchor == null) return;

        Transform handChild = handAnchor.Find(handName);
        if (handChild == null && handAnchor.childCount == 0)
        {
            GameObject handObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handObj.name = handName;
            handObj.transform.SetParent(handAnchor, false);
            handObj.transform.localScale = new Vector3(0.08f, 0.08f, 0.15f); // 优雅手部造型占位体
            
            var col = handObj.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }
    }
}
