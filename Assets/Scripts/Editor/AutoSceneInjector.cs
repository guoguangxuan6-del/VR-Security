using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景 YAML 实体硬注入器：
/// 在 Unity 编译完成后自动运行，直接将全套双手锚点、物理防卡墙、HDRP材质修复、
/// 患者人体模型、医学专家 3D 面板与 CPR 按压黄金区间管理器【真真实实写入场景 Demonstration.unity 的 YAML 磁盘文件中】！
/// </summary>
[InitializeOnLoad]
public class AutoSceneInjector
{
    static AutoSceneInjector()
    {
        EditorApplication.delayCall += PerformSceneInjection;
    }

    [MenuItem("Tools/Force Inject & Save Scene YAML")]
    public static void PerformSceneInjection()
    {
        Scene activeScene = EditorSceneManager.GetActiveScene();
        if (!activeScene.isLoaded || !activeScene.name.Equals("Demonstration"))
        {
            // 如果没打开 Demonstration，尝试打开
            string scenePath = "Assets/Scenes/Subway/Demonstration.unity";
            if (System.IO.File.Exists(scenePath))
            {
                activeScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
        }

        if (!activeScene.isLoaded) return;

        bool isModified = false;

        // 1. 硬写入 InputManager
        var inputMgr = Object.FindObjectOfType<InputManager>();
        if (inputMgr == null)
        {
            GameObject mgrObj = new GameObject("InputManager");
            inputMgr = mgrObj.AddComponent<InputManager>();
            Undo.RegisterCreatedObjectUndo(mgrObj, "Inject InputManager");
            isModified = true;
        }

        // 2. 硬写入 OVRCameraRig / VRPlayer
        var cameraRig = Object.FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null)
        {
            if (cameraRig.GetComponent<HDRPMaterialFixer>() == null)
            {
                cameraRig.gameObject.AddComponent<HDRPMaterialFixer>();
                isModified = true;
            }

            if (inputMgr != null)
            {
                inputMgr.SetHandAnchors(cameraRig.leftHandAnchor, cameraRig.rightHandAnchor);
            }

            GameObject playerRoot = cameraRig.transform.parent != null ? cameraRig.transform.parent.gameObject : null;
            if (playerRoot == null)
            {
                playerRoot = new GameObject("VRPlayer");
                playerRoot.transform.position = cameraRig.transform.position;
                cameraRig.transform.SetParent(playerRoot.transform);
                Undo.RegisterCreatedObjectUndo(playerRoot, "Inject VRPlayer Root");
                isModified = true;
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
        }

        // 3. 硬写入 Patient 人体受害者节点 (含 3D 对话面板)
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

            Undo.RegisterCreatedObjectUndo(patientObj, "Inject Patient");
            isModified = true;
        }

        // 4. 硬写入 CPR 按压黄金区间训练管理器
        var cprMgr = Object.FindObjectOfType<CPRTrainingManager>();
        if (cprMgr == null)
        {
            GameObject cprObj = new GameObject("CPRTrainingManager");
            cprMgr = cprObj.AddComponent<CPRTrainingManager>();
            Undo.RegisterCreatedObjectUndo(cprObj, "Inject CPRTrainingManager");
            isModified = true;
        }

        // 5. 核心保存：直接将场景真实写入 Demonstration.unity 磁盘文件！
        if (isModified)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Debug.Log("[AutoSceneInjector] Demonstration.unity YAML file updated & saved with full VR and CPR entities!");
        }
    }
}
