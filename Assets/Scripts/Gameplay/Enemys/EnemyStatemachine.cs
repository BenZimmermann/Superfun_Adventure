using UnityEngine;

public class EnemyStatemachine
{
    public EnemyState CurrentState { get; private set; }

    // Initialisiert die State Machine mit einem Start-State
    public void Initialize(EnemyState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    // Wechselt zu einem neuen State
    public void ChangeState(EnemyState newState)
    {
        if (CurrentState != null)
        {
            CurrentState.Exit();
        }

        CurrentState = newState;
        CurrentState.Enter();
    }

    // Update für den aktuellen State
    public void Update()
    {
        if (CurrentState != null)
        {
            CurrentState.Update();
        }
    }

    // FixedUpdate für den aktuellen State
    public void FixedUpdate()
    {
        if (CurrentState != null)
        {
            CurrentState.FixedUpdate();
        }
    }
}