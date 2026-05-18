using UnityEngine;

public class GameStateMachine : MonoBehaviour
{
    public static GameStateMachine Instance { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Transition(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameStateMachine] State -> {newState}");
    }
}