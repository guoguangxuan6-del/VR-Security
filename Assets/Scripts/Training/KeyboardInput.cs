using UnityEngine;

namespace LifeGuard.Training
{
    public class KeyboardInput : IInputSource
    {
        private float _holdTimer = 0f;
        private const float MaxDepthTime = 0.8f; // 按住 0.8 秒视为最大深度
        private const float DecayRate = 3f;       // 松手后回弹速度

        public bool IsReady => true;

        public float GetCompressionDepth01()
        {
            if (Input.GetKey(KeyCode.C))
            {
                _holdTimer += Time.deltaTime;
                return Mathf.Clamp01(_holdTimer / MaxDepthTime);
            }
            else
            {
                _holdTimer = Mathf.Max(0f, _holdTimer - DecayRate * Time.deltaTime);
                return 0f; // 松手后立即视为 0（回弹由 CompressionDetector 判定）
            }
        }

        public bool GetInteractDown() => Input.GetKeyDown(KeyCode.E);

        public Vector2 GetMovement()
        {
            float h = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
            float v = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
            return new Vector2(h, v);
        }

        public bool GetCancel() => Input.GetKeyDown(KeyCode.Escape);
    }
}
