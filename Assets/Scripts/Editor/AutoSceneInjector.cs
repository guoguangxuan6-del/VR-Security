using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// 全套 VRPlayer 根节点与绑定的双手硬重建/注入器
/// 自动重建 VRPlayer 根物体，重构 OVRCameraRig 与双手 (LeftHandAnchor / RightHandAnchor)，
/// 并挂载 VRPlayerRig 行为控制、HDRP 材质修复与双手输入，直接 SaveScene 保存写入 Demonstration.unity YAML!
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
            // 尝试在场景中生成 OVRCameraRig
            GameObject cameraRigObj = new GameObject("OVRCameraRig");
            cameraRig = cameraRigObj.AddComponent<OVRCameraRig>();
            
            // 构建 TrackingSpace 层级
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

        // 3. 彻底重构并恢复【VRPlayer 根节点与绑定的双手】
        GameObject playerRoot = GameObject.Find("VRPlayer");
        if (playerRoot == null)
        {
            // 如果 OVRCameraRig 的父节点不是 VRPlayer，新建 VRPlayer 根物体
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

        // 给 VRPlayer 挂载 CharacterController
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

        // 给 VRPlayer 挂载 VRPlayerRig 行走与平滑旋转逻辑
        var playerRig = playerRoot.GetComponent<VRPlayerRig>();
        if (playerRig == null)
        {
            playerRig = playerRoot.AddComponent<VRPlayerRig>();
            isModified = true;
        }

        SerializedObject serializedRig = new SerializedObject(playerRig);
        serializedRig.FindProperty("cameraRig").objectReferenceValue = cameraRig;
        serializedRig.ApplyModifiedProperties();

        // 挂载 HDRP 材质修复器 (确保双手显形)
        if (cameraRig.GetComponent<HDRPMaterialFixer>() == null)
        {
            cameraRig.gameObject.AddComponent<HDRPMaterialFixer>();
            isModified = true;
        }

        // 绑定双手 Anchor 给 InputManager
        if (inputMgr != null && cameraRig != null)
        {
            inputMgr.SetHandAnchors(cameraRig.leftHandAnchor, cameraRig.rightHandAnchor);
        }

        // 4. 重建/确保 Patient 人体模型节点 (包含医学专家 3D 对话面板)
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

        // 5. 重载 CPR 按压黄金区间训练管理器
        var cprMgr = Object.FindObjectOfType<CPRTrainingManager>();
        if (cprMgr == null)
        {
            GameObject cprObj = new GameObject("CPRTrainingManager");
            cprMgr = cprObj.AddComponent<CPRTrainingManager>();
            Undo.RegisterCreatedObjectUndo(cprObj, "Create CPRTrainingManager");
            isModified = true;
        }

        // 保存变动，强制持久化写进 Demonstration.unity YAML 文本！
        if (isModified)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Debug.Log("[AutoSceneInjector] Successfully reconstructed VRPlayer, Hands & CPR Entities in Demonstration.unity!");
        }
    }
}
