using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 场景设置与人体模型一键插入助手
/// 在 Unity 编辑器顶部菜单 Tools > VR Setup Helper 中使用
/// </summary>
public class SceneSetupHelper : EditorWindow
{
    [MenuItem("Tools/VR Setup Helper")]
    public static void ShowWindow()
    {
        GetWindow<SceneSetupHelper>("VR Setup Helper");
    }

    void OnGUI()
    {
        GUILayout.Label("VR 急救培训 - 人体模型与场景助手", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("一键向当前场景添加急救人体 (Patient)", GUILayout.Height(40)))
        {
            AddPatientToActiveScene();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "提示：点击上方按钮会自动在场景中创建 Patient 人体模型挂载点。\n" +
            "你可以将你放置的 FBX 人体模型拖入 Patient 节点作为子物体，" +
            "即可完美体验急救发病与 VR 按压训练！",
            MessageType.Info);
    }

    static void AddPatientToActiveScene()
    {
        string folder = "Assets/Prefabs/Game";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        // 查找场景中是否已有 Patient
        GameObject patientObj = GameObject.Find("Patient");
        if (patientObj == null)
        {
            patientObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            patientObj.name = "Patient";
            patientObj.transform.position = new Vector3(0f, 0f, 2.5f); // 默认摆放在玩家面前 2.5 米处
            patientObj.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);

            var animator = patientObj.GetComponent<Animator>();
            if (animator == null) patientObj.AddComponent<Animator>();

            patientObj.AddComponent<PatientController>();

            Undo.RegisterCreatedObjectUndo(patientObj, "Create Patient");
            Selection.activeGameObject = patientObj;

            EditorUtility.DisplayDialog("成功", "已成功在场景中创建 Patient (受害者人体)！\n位置：玩家前方 2.5 米处。\n你可以将你的 FBX 人体模型拖入其下方作为 Mesh。", "OK");
        }
        else
        {
            Selection.activeGameObject = patientObj;
            EditorUtility.DisplayDialog("提示", "场景中已经存在名为 'Patient' 的人体模型。已自动为你选中该物体。", "OK");
        }
    }
}
