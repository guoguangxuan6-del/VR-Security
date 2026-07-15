namespace LifeGuard.Training
{
    /// <summary>
    /// Input abstraction: single source of truth for keyboard / VR / any future device.
    /// Business code MUST only see this interface.
    /// </summary>
    public interface IInputSource
    {
        ///summary>
        /// Compression depth, 0 (no press) → 1 (max). Keyboard: maps C hold duration.
        /// </summary>
        float GetCompressionDepth01();

        /// <summary>
        /// Interact / confirm / tap-shoulder: true only on the frame key goes down.
        /// </summary>
        bool GetInteractDown();

        /// <summary>
        /// Movement vector (-1..1, -1..1). Keyboard: WASD.
        /// </summary>
        UnityEngine.Vector2 GetMovement();

        /// <summary>
        /// Cancel / back. Keyboard: Escape.
        /// </summary>
        bool GetCancel();

        /// <summary>
        /// Whether this source is ready. Keyboard always ready; VR needs controllers connected.
        /// </summary>
        bool IsReady { get; }
    }
}
