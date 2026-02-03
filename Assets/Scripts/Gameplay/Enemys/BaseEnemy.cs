using UnityEngine;
using System;

public class BaseEnemy : MonoBehaviour, IDamageable
{
    [Header("Enemy Settings")]
    [SerializeField] private EnemyData enemyData;
    
    private int currentHealth;
    private PlayerMovement player;
    private Transform playerTransform;
    private Rigidbody2D rb;
    
    // Event für Tod des Gegners
    public event Action OnDeath;
    
    // Public Properties
    public int CurrentHealth => currentHealth;
    public EnemyData EnemyData => enemyData;
    public Rigidbody2D Rb => rb;
    
    // State Machine
    public EnemyStatemachine StateMachine { get; private set; }
    
    // States (public für einfachen Zugriff aus anderen States)
    public EnemyState.IdleState IdleState { get; private set; }
    public EnemyState.MoveState MoveState { get; private set; }
    public EnemyState.ChaseState ChaseState { get; private set; }
    public EnemyState.AttackState AttackState { get; private set; }
    public EnemyState.DieState DieState { get; private set; }
    
    private void Awake()
    {
        // Initialisiere Health aus EnemyData
        if (enemyData != null)
        {
            currentHealth = enemyData.maxHealth;
        }
        else
        {
            Debug.LogWarning($"EnemyData fehlt bei {gameObject.name}!");
            currentHealth = 10; // Fallback
        }
        
        // Hole Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"Rigidbody2D fehlt bei {gameObject.name}! Füge eine Rigidbody2D Komponente hinzu.");
        }
        
        // Finde und cache den Player
        player = FindFirstObjectByType<PlayerMovement>();
        
        if (player == null)
        {
            Debug.LogWarning("PlayerMovement nicht gefunden! OnEnemyKilled() kann nicht aufgerufen werden.");
        }
        else
        {
            playerTransform = player.transform;
        }
        
        // Initialisiere State Machine
        StateMachine = new EnemyStatemachine();
        
        // Erstelle alle States
        IdleState = new EnemyState.IdleState(StateMachine, this);
        MoveState = new EnemyState.MoveState(StateMachine, this);
        ChaseState = new EnemyState.ChaseState(StateMachine, this);
        AttackState = new EnemyState.AttackState(StateMachine, this);
        DieState = new EnemyState.DieState(StateMachine, this);
        
        // Starte mit Idle State
        StateMachine.Initialize(IdleState);
    }
    
    private void Update()
    {
        // Update den aktuellen State
        StateMachine.Update();
    }
    
    private void FixedUpdate()
    {
        // FixedUpdate für den aktuellen State
        StateMachine.FixedUpdate();
    }
    
    public void TakeDamage(int amount, DamageSource source)
    {
        if (currentHealth <= 0)
            return;
        
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        Debug.Log($"{gameObject.name} took {amount} damage from {source}. Health: {currentHealth}/{enemyData.maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        
        // Wechsle zu Die State
        StateMachine.ChangeState(DieState);
        
        // Event auslösen
        OnDeath?.Invoke();
        
        // Rufe OnEnemyKilled() beim Player auf (reduziert Psychosis)
        if (player != null)
        {
            player.OnEnemyKilled();
        }
    }
    
    #region Helper Methods für States
    
    /// <summary>
    /// Prüft ob der Player in Detection Range ist
    /// </summary>
    public bool IsPlayerInDetectionRange()
    {
        if (playerTransform == null || enemyData == null)
            return false;
        
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        return distance <= enemyData.detectionRange;
    }
    
    /// <summary>
    /// Prüft ob der Player in Attack Range ist
    /// </summary>
    public bool IsPlayerInAttackRange()
    {
        if (playerTransform == null || enemyData == null)
            return false;
        
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        return distance <= enemyData.attackRange;
    }
    
    /// <summary>
    /// Gibt die Distanz zum Player zurück
    /// </summary>
    public float GetDistanceToPlayer()
    {
        if (playerTransform == null)
            return float.MaxValue;
        
        return Vector2.Distance(transform.position, playerTransform.position);
    }
    
    #endregion
    
    #region Debug Visualization
    
    private void OnDrawGizmosSelected()
    {
        if (enemyData == null)
            return;
        
        // Detection Range (gelb)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.detectionRange);
        
        // Attack Range (rot)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
    }
    
    #endregion
}