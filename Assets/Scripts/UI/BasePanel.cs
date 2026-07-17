using UnityEngine;

public abstract class BasePanel : MonoBehaviour
{
    public virtual void OnEnter(object data) { }
    public virtual void OnExit() { }

    public virtual void OnBack()
    {
        UIManager.Instance.GoBack();
    }

    protected T Bind<T>(string name) where T : Component
    {
        Transform child = transform.Find(name);
        if (child == null)
        {
            Debug.LogError($"[{GetType().Name}] Child '{name}' not found");
            return null;
        }
        return child.GetComponent<T>();
    }
}
