using UnityEngine;

public class EnemyStatemachine
{
    private EnemyState currentState;
    public EnemyState CurrentState => currentState;
    //public EnemyState CurrentState { get; private set; }

    // Initialisiert die State Machine mit einem Start-State
    public void Initialize(EnemyState startingState)
    {
        if (startingState == null)
        {
            Debug.LogError("[EnemyStatemachine] Cannot initialize with null state!");
            return;
        }

        currentState = startingState;
        currentState.Enter();
    }

    // Wechselt zu einem neuen State
    public void ChangeState(EnemyState newState)
    {
        if (newState == null)
        {
            Debug.LogError("[EnemyStatemachine] Cannot change to null state!");
            return;
        }

        if (currentState == newState)
        {
            return; // Already in this state
        }

        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }
        // Update für den aktuellen State
        public void Update()
    {
        currentState?.Update();
    }

    // FixedUpdate für den aktuellen State
    public void FixedUpdate()
    {
        currentState?.FixedUpdate();
    }
}