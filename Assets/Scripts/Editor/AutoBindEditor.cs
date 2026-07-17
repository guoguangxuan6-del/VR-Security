using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Text.RegularExpressions;

[CustomEditor(typeof(MonoBehaviour), true)]
public class AutoBindEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MonoBehaviour script = (MonoBehaviour)target;
        if (GUILayout.Button("Auto Bind Fields"))
        {
            AutoBindFields(script);
        }
    }

    void AutoBindFields(MonoBehaviour script)
    {
        Transform t = script.transform;
        FieldInfo[] fields = script.GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        int bound = 0;
        foreach (FieldInfo field in fields)
        {
            if (!field.IsDefined(typeof(SerializeField), true))
                continue;

            object currentValue = field.GetValue(script);
            if (currentValue != null && !IsDefaultValue(currentValue))
                continue;

            string childName = FieldNameToChildName(field.Name);
            Transform child = t.Find(childName);
            if (child == null)
            {
                child = TryFindNested(t, childName);
            }

            if (child == null) continue;

            System.Type fieldType = field.FieldType;
            if (fieldType == typeof(GameObject))
            {
                field.SetValue(script, child.gameObject);
                bound++;
            }
            else if (typeof(Component).IsAssignableFrom(fieldType))
            {
                Component comp = child.GetComponent(fieldType);
                if (comp != null)
                {
                    field.SetValue(script, comp);
                    bound++;
                }
            }
        }

        if (bound > 0)
        {
            EditorUtility.SetDirty(script);
            Debug.Log($"[AutoBind] {script.GetType().Name}: {bound} fields bound");
        }
        else
        {
            Debug.Log($"[AutoBind] {script.GetType().Name}: no unbound fields found");
        }
    }

    bool IsDefaultValue(object value)
    {
        if (value == null) return true;
        if (value is Object obj && obj == null) return true;
        var type = value.GetType();
        if (type.IsValueType)
            return value.Equals(System.Activator.CreateInstance(type));
        return false;
    }

    string FieldNameToChildName(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName)) return fieldName;
        return char.ToUpper(fieldName[0]) + fieldName.Substring(1);
    }

    Transform TryFindNested(Transform root, string name)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == name)
                return child;
            Transform found = TryFindNested(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}
