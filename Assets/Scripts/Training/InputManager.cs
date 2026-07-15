using UnityEngine;

namespace LifeGuard.Training
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public IInputSource Current { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 默认键盘，VR 连接后在 ME-08 中替换
            Current = new KeyboardInput();
        }

        /// <summary>
        /// ME-08 中调用：切换到 VR 输入源
        /// </summary>
        public void SetSource(IInputSource source)
        {
            Current = source;
        }
    }
}
