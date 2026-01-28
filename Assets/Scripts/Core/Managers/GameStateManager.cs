using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    /// <summary>
    /// -set state
    /// -get state
    /// -change state
    /// </summary>
    public static GameStateManager Instance { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.Boot;
    private GameState previousState;
    public event Action<GameState, GameState> OnStateChanged;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void SetState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        //GameState previousState = CurrentState;
        //CurrentState = newState;
        previousState = CurrentState;
        CurrentState = newState;

        Debug.Log($"[GameStateManager] State changed: {previousState} -> {newState}");

        OnStateChanged?.Invoke(previousState, newState);
    }
    public void LastGameState()
    {
        SetState(previousState);
    }
    public GameState GetState()
    {
        return CurrentState;
    }
    public bool IsInState(GameState state)
    {
        return CurrentState == state;
    }
      
}
public enum GameState
    {
        Boot,
        MainMenu,
        Playing,
        Paused,
        BossFight,
        GameOver,
        Corrupted,
        Settings
    }